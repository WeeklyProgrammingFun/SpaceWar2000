using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRunner.SpaceWar2K
{
    // hold one planet
    public struct Planet
    {
        public Planet(Planet p, int population, Owner owner)
        {
            index_ = p.index_;
            x_ = p.x_;
            y_ = p.y_;
            owner_ = owner;
            growthRate_ = p.growthRate_;
            population_ = population;
        }
        public int index_ { get; set; }
        // position
        public double x_, y_;
        public Owner owner_;
        public int growthRate_ { get; set; }
        public int population_ { get; set; }

        public override string ToString()
        {
            return $"{index_} {x_} {y_} {owner_} {population_} {growthRate_}";
        }

    }
}
