using BotBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathStarBot
{
    class DeathStarBot : BotBase.BotBase
    {
        // every few moves. fire all items at one non-my planet
        protected override void GenMoves()
        {
            // try attack every 5th frame
            var attackId = -1;
            var myPlanets = MyPlanets();
            if (Planets.Count > myPlanets.Count && (Frame%10)==0)
            {
                var passMax = 100;
                var pass = 0;
                do
                {
                    attackId = Rand.Next(Planets.Count);
                    ++pass;
                } while (pass < passMax && Planets[attackId].owner_ == Owner.Player1);

                if (pass < passMax)
                {
                    // have planet to attack. Launch all
                    foreach (var p in myPlanets)
                        LaunchFleet(p.id_, attackId, p.population_);
                }
            }
        }

        static void Main(string[] args)
        {
            new DeathStarBot().Run(args);
        }
    }
}
