using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRunner.SpaceWar2K
{
    // a game state
    // treat as immutable
    public class State
    {
        public State(int turn = 1, int maxTurn = 200, List<Planet> planets = null, List<Fleet> fleets = null)
        {
            turn_ = turn;
            turnMax_ = maxTurn;
            planets_ = planets;
            fleets_ = fleets;
        }

        // count turns
        public int turn_;
        // max number of turns
        public int turnMax_ = 200;

        public List<Planet> planets_ = new List<Planet>();
        public List<Fleet> fleets_ = new List<Fleet>();

        // fleets en route to planet
        public int FleetsEnRoute(Owner owner, Planet planet)
        {
            var index = planet.index_;
            return fleets_.Where(f => f.owner_ == owner && f.dst_ == index).Sum(f=>f.population_);
        }

        public int Population(int owner)
        {
            return planets_.Where(p=> (int)p.owner_ == owner).Select(p => p.population_).Sum() + 
                fleets_.Where(p => (int)p.owner_ == owner).Select(p => p.population_).Sum();
        }
        public int GrowthRate(int owner)
        {
            return planets_.Where(p => (int) p.owner_ == owner).Select(p => p.growthRate_).Sum();
        }

    }

}
