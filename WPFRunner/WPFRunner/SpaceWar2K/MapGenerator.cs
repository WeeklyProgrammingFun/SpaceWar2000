using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRunner.SpaceWar2K
{
    public class MapGenerator
    {
        /// <summary>
        /// Make a list of initial planets
        /// </summary>
        /// <param name="randomSeed"></param>
        /// <returns></returns>
        public List<Planet> Make(int randomSeed)
        {
            this.random = new Random(randomSeed);
            this.planets = new List<Planet>();

            // works out information about the map
            var planetsToGenerate = random.Next(minPlanets, maxPlanets);

            int symmetryType;
            if (random.Next(1) == 0)
            {
                symmetryType = 1;
                // radial symmetry
                // can only generate an odd number of planets in this symmetry
                while (planetsToGenerate % 2 == 0)
                {
                    if (planetsToGenerate == maxPlanets)
                        planetsToGenerate = minPlanets;
                    else
                        planetsToGenerate += 1;
                }
            }
            else
            {
                //  linear symmetry
                symmetryType = -1;
            }

            // adds the center planet
            planets.Add(MakePlanet(0, 0, 0, random.Next(minShips, maxShips), random.Next(0, maxGrowth)));
            planetsToGenerate -= 1;

            // picks out the home planets
            var r = RandRadius(minDistance, maxRadius);
            var theta1 = RandDouble(0, Math.PI * 2);
            double theta2;
            if (symmetryType == 1 && theta1 < Math.PI)
                theta2 = theta1 + Math.PI;
            else if (symmetryType == 1)
                theta2 = theta1 - Math.PI;
            else
                theta2 = RandDouble(0, Math.PI * 2);

            var p1 = MakePlanet(0, 0, Owner.Player1, 100, 5);
            var p2 = MakePlanet(0, 0, Owner.Player2, 100, 5);
            p1 = GenerateCoordinates1(p1, r, theta1);
            p2 = GenerateCoordinates1(p2, r, theta2);

            while (NotValid(p1, p2) || Distance(p1, p2) < minStartingDistance)
            {
                r = RandRadius(minDistance, maxRadius);
                theta1 = RandDouble(0, Math.PI * 2);
                if (symmetryType == 1 && theta1 < Math.PI)
                    theta2 = theta1 + Math.PI;
                else if (symmetryType == 1)
                    theta2 = theta1 - Math.PI;
                else
                    theta2 = RandDouble(0, Math.PI * 2);

                p1 = GenerateCoordinates1(p1, r, theta1);
                p2 = GenerateCoordinates1(p2, r, theta2);
            }

            planets.Add(p1);
            planets.Add(p2);
            planetsToGenerate -= 2;

            // makes the center neutral planets
            if (symmetryType == 1)
            {
                var noCenterNeutrals = 2 * random.Next(0, maxCentral / 2);
                var thetaA = (theta1 + theta2) / 2;
                var thetaB = thetaA + Math.PI;
                for (var i = 0; i < noCenterNeutrals / 2; ++i)
                {
                    r = RandRadius(minDistance, maxRadius);
                    var num_ships = random.Next(minShips, maxShips);
                    var growth_rate = random.Next(minGrowth, maxGrowth);
                    p1 = MakePlanet(0, 0, 0, num_ships, growth_rate);
                    p2 = MakePlanet(0, 0, 0, num_ships, growth_rate);
                    p1 = GenerateCoordinates1(p1, r, thetaA);
                    p2 = GenerateCoordinates1(p2, r, thetaB);
                    while (NotValid(p1, p2))
                    {
                        r = RandRadius(minDistance, maxRadius);
                        p1 = GenerateCoordinates1(p1, r, thetaA);
                        p2 = GenerateCoordinates1(p2, r, thetaB);
                    }

                    planets.Add(p1);
                    planets.Add(p2);
                    planetsToGenerate -= 2;

                }
            }
            else
            {
                // must have an even number of planets left to generate after this
                var minCentral = planetsToGenerate % 2;
                var noCenterNeutrals = random.Next(minCentral, maxCentral + 1);
                if (noCenterNeutrals % 2 != minCentral)
                    noCenterNeutrals -= 1;
                var theta = (theta1 + theta2) / 2;
                if (random.Next(1) == 0)
                    theta += Math.PI;
                for (var i = 0; i < noCenterNeutrals; ++i)
                {
                    r = RandRadius(0, maxRadius);
                    var num_ships = random.Next(minShips, maxShips);
                    var growth_rate = random.Next(minGrowth, maxGrowth);
                    var p = MakePlanet(0, 0, 0, num_ships, growth_rate);
                    p = GenerateCoordinates1(p, r, theta);
                    while (NotValids(p))
                    {
                        r = RandRadius(0, maxRadius);
                        p = GenerateCoordinates1(p, r, theta);
                    }

                    planets.Add(p);
                    planetsToGenerate -= 1;
                }
            }

            // picks out the rest of the neutral planets
            Debug.Assert(planetsToGenerate % 2 == 0, "Error: odd number of planets left to add");
            for (var i = 0; i < planetsToGenerate / 2; ++i)
            {
                r = RandRadius(minDistance, maxRadius);
                var theta = RandDouble(0, Math.PI * 2);
                int num_ships, planet_max;
                if (i == 0)
                {
                    planet_max = (int)Math.Min(100, 5 * Distance(planets[1], planets[2]) - 1);
                    num_ships = random.Next(minShips, planet_max);
                }
                else
                    num_ships = random.Next(minShips, maxShips);

                var growth_rate = random.Next(minGrowth, maxGrowth);
                p1 = MakePlanet(0, 0, 0, num_ships, growth_rate);
                p2 = MakePlanet(0, 0, 0, num_ships, growth_rate);
                p1 = GenerateCoordinates1(p1, r, theta1 + theta);
                p2 = GenerateCoordinates1(p2, r, theta2 + symmetryType * theta);

                while (NotValid(p1, p2))
                {
                    r = RandRadius(minDistance, maxRadius);
                    theta = RandDouble(0, Math.PI * 2);
                    p1 = GenerateCoordinates1(p1, r, theta1 + theta);
                    p2 = GenerateCoordinates1(p2, r, theta2 + symmetryType * theta);
                }

                planets.Add(p1);
                planets.Add(p2);
            }


            // translate planets
            return planets.Select((p,i) => new Planet(p,p.population_, p.owner_) {index_ = i, x_ = p.x_ + maxRadius, y_ = p.y_+maxRadius }).ToList();
        }

        #region Implementation
        // minimum and maximum total number of planets in map
        private const int minPlanets = 15;

        private const int maxPlanets = 30;

        // maximum number of planets specifically generated to be equidistant from both
        // players, by chance planet generated in the standard symmetric way could still
        // end up equidistant as well
        // also does not include the planet exactly in the center of the map
        private const int maxCentral = 5;

        // minimum and maximum number of ships on neutral planets
        private const int minShips = 1;

        private const int maxShips = 100;

        // minimum and maximum growth for planets
        // except for the center planet which is always 0 minimum growth
        private const int minGrowth = 1;

        private const int maxGrowth = 5;

        // minimum distance between planets
        private const int minDistance = 2;

        // minimum distance between the players starting planets
        private const int minStartingDistance = 4;

        // maximum radius from center of map min planet can be
        private const int maxRadius = 15;

        // minimum difference between true distance and rounded distance between planets
        // this is to try and avoid rounding errors causing different distances to be
        // calculated on different platforms and languages
        private const double epsilon = 0.002;

        Planet MakePlanet(double x, double y, Owner owner, int num_ships, int growth_rate)
        {
            return new Planet
            {
                x_ = x,
                y_ = y,
                growthRate_ = growth_rate,
                owner_ = owner,
                population_ = random.Next(5*growth_rate,12*growth_rate)+growth_rate
            };
        }

        double RandDouble(double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }

        Planet GenerateCoordinates1(Planet p, double r, double theta)
        {
            if (theta < 0)
                theta += Math.PI * 2;
            if (theta >= Math.PI * 2)
                theta -= Math.PI * 2;

            return MakePlanet(r*Math.Cos(theta),r*Math.Sin(theta),p.owner_, p.population_, p.growthRate_);
        }

        private Random random;
        private List<Planet> planets;

        double RandRadius(double min_r, double max_r)
        {
            var val = min_r - 1;
            while (val < min_r)
                val = Math.Sqrt(random.NextDouble()) * max_r;
            return val;
        }

        double Distance(Planet p1, Planet p2)
        {
            return Math.Ceiling(ActualDistance(p1, p2));
        }

        double ActualDistance(Planet p1, Planet p2)
        {
            var dx = p1.x_ - p2.x_;
            var dy = p1.y_ - p2.y_;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        bool NotValid(Planet p1, Planet p2)
        {
            var adist = ActualDistance(p1, p2);
            if (Distance(p1, p2) < minDistance || Math.Abs(adist - Math.Round(adist)) < epsilon)
                return true;
            foreach (var p in planets)
            {
                var adist1 = ActualDistance(p, p1);
                var adist2 = ActualDistance(p, p2);
                if (Distance(p, p1) < minDistance || Distance(p, p2) < minDistance ||
                    Math.Abs(adist1 - Math.Round(adist1)) < epsilon ||
                    Math.Abs(adist2 - Math.Round(adist2)) < epsilon)
                    return true;
            }

            return false;
        }

        bool NotValids(Planet p1)
        {
            foreach (var p in planets)
            {
                var adist = ActualDistance(p, p1);
                if (Distance(p, p1) < minDistance || Math.Abs(adist - Math.Round(adist)) < epsilon)
                    return true;
            }

            return false;
        }
        #endregion

    }
}
