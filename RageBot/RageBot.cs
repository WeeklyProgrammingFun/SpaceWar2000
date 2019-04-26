using BotBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RageBot
{
    class RageBot : BotBase.BotBase
    {
        /// <summary>
        /// Do one move given the planet wars state
        /// </summary>
        protected override void GenMoves()
        {
            foreach (var source in MyPlanets())
            {
                if (source.population_ < 10 * source.growthRate_)
                {
                    continue;
                }
                Planet dest = null;
                int bestDistance = 999999;
                foreach (var p in EnemyPlanets())
                {
                    int dist = Distance(source, p);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        dest = p;
                    }
                }
                // find closest - todo - find linq way
                //                var mm = EnemyPlanets().Select((p, n) => new { d = Distance(p, source), n = n }).(a => a.d);
                if (dest != null)
                {
                    LaunchFleet(source.id_, dest.id_, source.population_);
                }
            }
        }

        static void Main(string[] args)
        {
            new RageBot().Run(args);
        }
    }
}
