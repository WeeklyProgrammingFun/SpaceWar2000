using BotBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomBot
{
    class RandomBot : BotBase.BotBase
    {
        protected override void GenMoves()
        {
            // if I have no planets left, do nothing
            var myPlanets = MyPlanets();
            if (myPlanets.Count == 0)
                return;

            var planetCount = Planets.Count;
            foreach (var p in myPlanets)
            {
                var pop = p.population_;
                //while (pop > 0)
                {
                    var dst  = Rand.Next(planetCount);
                    var send = Rand.Next(pop/2) + (pop+1)/2;
                    LaunchFleet(p.id_, dst, send);
                    pop -= send;
                }
            }
        }

        static void Main(string[] args)
        {
            new RandomBot().Run(args);
        }
    }
}
