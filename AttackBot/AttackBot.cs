using BotBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackBot
{
    class AttackBot : BotBase.BotBase
    {
        protected override void GenMoves()
        {
            // if I have no planets left, do nothing
            var myPlanets = MyPlanets();
            if (myPlanets.Count == 0)
                return;

            // find my strongest planet
            var strongest = myPlanets.OrderBy(p => p.population_).Last();

            // find my fleet destinations
            var dests = Fleets.Select(f => f.dst_);

            // Get enemy planets not under attack any of my fleets
            var enemyPlanets = EnemyPlanets().Where(p => !dests.Contains(p.id_));
            if (enemyPlanets.Count() > 0)
            {
                // find weakest (not under attack by me) and attack if possible
                var weakest = enemyPlanets.OrderBy(p => p.population_).First();
                var dist = Distance(weakest, strongest);
                int needed = weakest.population_ + (dist + 1) * weakest.growthRate_ + 1;
                if (strongest.population_ >= needed)
                {
                    // we can kill it - do so.
                    LaunchFleet(strongest.id_, weakest.id_, needed);
                }
            }

            if (Commands.Count == 0)
            {
                // no command yet, kill any neutral planet (not under attack already) if possible
                var neutral = NeutralPlanets().Where(p => !dests.Contains(p.id_));
                if (neutral.Count() > 0)
                {
                    var weakest = neutral.OrderBy(p => p.population_).First();
                    if (strongest.population_ > weakest.population_)
                    {
                        // we can kill it - do so.
                        LaunchFleet(strongest.id_, weakest.id_, weakest.population_ + 1);
                    }
                }
            }

            if ((Commands.Count == 0) && (myPlanets.Count > 1))
            {
                // no command yet, use second strongest planet to feed strongest planet
                var second = myPlanets.OrderByDescending(p => p.population_).Skip(1).First();
                LaunchFleet(second.id_, strongest.id_, second.population_ / 2);
            }

        }

        static void Main(string[] args)
        {
            new AttackBot().Run(args);
        }
    }
}
