using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WPFRunner.Model;

namespace WPFRunner.SpaceWar2K
{

    /// <summary>
    /// Update game step one step
    /// </summary>
    public static class StateUpdater
    {

        // update the state given the list of launches
        // return result of update
        // fleets need owner, src, dst, population filled in
        public static (Result, State) Update(List<Launch> launches, State state1)
        {
            /*
             * 1. Departure (fleets leave)
             * 2. Advancement (fleet move, pop growth)
             * 3. Arrival (land and battle)
             */

            var planetCount = state1.planets_.Count;

            // 0. validate
            var invalidLaunch = launches
                // invalid lauch src, dst, owner
                .Where(launch =>
                    launch.src_ < 0 || planetCount <= launch.src_ ||
                    launch.dst_ < 0 || planetCount <= launch.dst_ ||
                    state1.planets_[launch.src_].owner_ != launch.owner_
                    )
                .Select(inv => inv.owner_)
                // invalid population totals
                .Concat(launches
                    .GroupBy(launch => launch.src_)
                    .Where(gp =>
                        gp.Sum(launch => launch.population_) > state1.planets_[gp.First().src_].population_)
                    .Select(gp => gp.First().owner_)
                )
                .ToList();
            if (invalidLaunch.Any())
            { // first loses, even if other illegal too
                var owner = invalidLaunch[0];
                var enemyWin = owner == Owner.Player1 ? Result.Player2Win : Result.Player1Win;
                return (enemyWin, state1);
            }

            // 1. Launch fleets (launch with -1 turns to perform initial move, population done next stage)
            var newFleets = launches.Select(launch =>
            {
                var p1 = state1.planets_[launch.src_];
                var p2 = state1.planets_[launch.dst_];
                var dist = Distance(p1,p2);
                return new Fleet
                {
                    owner_ = launch.owner_,
                    src_ = launch.src_,
                    dst_ = launch.dst_,
                    population_ = launch.population_,
                    totalTurns_ = dist,
                    turnsRemaining_ = dist - 1
                };
            }).ToList();

            // 2. advancement (fleets move, planets grow)
            var fleetLaunchPops = launches // dict of src=>value
                    .GroupBy(launch => launch.src_)
                    .Select(gp =>
                        (
                        gp.First().src_,
                        gp.Sum(launch => launch.population_)
                        ))
                     .ToDictionary(p => p.Item1, p => p.Item2);
            var fleets = state1.fleets_.Select(f => new Fleet(f, -1)).Concat(newFleets).ToList();
            var planets = state1.planets_.Select((p, i) => 
                new Planet(
                    p, 
                    p.population_ + (p.owner_ != Owner.None ? p.growthRate_ : 0) - (fleetLaunchPops.ContainsKey(i)?fleetLaunchPops[i]:0),
                    p.owner_)).ToList();

            // 3. land fleets, resolve battles
            var landingFleets = fleets.Where(f => f.turnsRemaining_ == 0).ToList();
            var remainingFleets = fleets.Where(f => f.turnsRemaining_ > 0).ToList();


            // given landing fleets, and a  list of planets, compute resulting list of planets
            /* for each (planet,index) (p,i):
             *    f1 = sum of player 1 landing
             *    f2 = sum of player 2 landing
             *    (owner,pop) = Resolve(p.owner,p.pop,f1.pop,f2.pop)
             *    make planet ()
             */
            var finalPlanets = planets.Select((p, i) =>
            {
                var f1 = landingFleets.Where(f => f.dst_ == i && f.owner_ == Owner.Player1).Sum(f => f.population_);
                var f2 = landingFleets.Where(f => f.dst_ == i && f.owner_ == Owner.Player2).Sum(f => f.population_);
                var (owner, pop) = Resolve(p.owner_, p.population_, f1, f2);
                return new Planet(p, pop, owner);
            }).ToList();


            // 4. Check endgame
            var state = new State(state1.turn_ + 1, state1.turnMax_, finalPlanets, remainingFleets);
            var result = CheckEndgame(state);

            return (result, state);
        }
        static Result CheckEndgame(State state)
        {
            var player1Population =
                state.fleets_.Where(f => f.owner_ == Owner.Player1).Select(f => f.population_).Sum() +
                state.planets_.Where(p => p.owner_ == Owner.Player1).Select(p => p.population_).Sum();
            var player1Planets = state.planets_.Where(p=> p.owner_ == Owner.Player1).Count();
            var player2Population =
                state.fleets_.Where(f => f.owner_ == Owner.Player2).Select(f => f.population_).Sum() +
                state.planets_.Where(p => p.owner_ == Owner.Player2).Select(p => p.population_).Sum();
            var player2Planets = state.planets_.Where(p => p.owner_ == Owner.Player2).Count();
            if (state.turn_ > state.turnMax_)
            {
                if (player1Population > player2Population)
                    return Result.Player1Win;
                if (player1Population < player2Population)
                    return Result.Player2Win;
                return Result.Tie;
            }

            if (player1Population + player1Planets <= 0 && player2Population > 0)
                return Result.Player2Win;
            if (player1Population > 0 && player2Population + player2Planets <= 0)
                return Result.Player1Win;
            if (player1Population + player1Planets <= 0 && player2Population + player2Planets <= 0)
                return Result.Tie;
            return Result.Unfinished;
        }

        /// <summary>
        /// Given a planet owner, planet population, landing fleet populations of player 1 and player 2, 
        /// compute the new owner and population
        /// </summary>
        static (Owner,int) Resolve(Owner owner, int pop, int f1, int f2)
        {
            // move planet population to a landing fleet if makes sense
            if (owner == SpaceWar2K.Owner.Player1)
            {
                f1 += pop;
                pop = 0;
            }
            if (owner == SpaceWar2K.Owner.Player2)
            {
                f2 += pop;
                pop = 0;
            }

            // resolve: messy, find biggest 2
            if (pop <= f1 && pop <= f2)
                return ComputeOwner(pop,f1,f2,owner,Owner.Player1, Owner.Player2);
            else if (f1 <= pop && f1 <= f2)
                return ComputeOwner(f1, pop, f2, owner, owner, Owner.Player2);
            else // if (f2 <= pop && f2 <= f1)
                return ComputeOwner(f2, pop, f1, owner, owner, Owner.Player1);
        }
        // if a smallest, b,c are players Pa,Pb
        static (Owner, int) ComputeOwner(int a, int b, int c, Owner original, Owner pB, Owner pC)
        {
            Debug.Assert(a <= b && a <= c);
            var m = Math.Min(b, c); // subtract smaller from larger
            b -= m;
            c -= m;
            if (b > 0) return (pB, b);
            if (c > 0) return (pC, c);
            return (original, 0);
        }


        /// <summary>
        /// Distance between planets
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static int Distance(Planet p1, Planet p2)
        {
            var dx = p1.x_ - p2.x_;
            var dy = p1.y_ - p2.y_;
            return (int) Math.Ceiling(Math.Sqrt(dx * dx + dy * dy));
        }
    }
}
