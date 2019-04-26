using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRunner.SpaceWar2K
{
    public struct Fleet
    {
        public Fleet(Fleet f, int deltaRemaining = 0)
        {
            src_ = f.src_;
            dst_ = f.dst_;
            owner_ = f.owner_;
            population_ = f.population_;
            totalTurns_ = f.totalTurns_;
            turnsRemaining_ = f.turnsRemaining_ + deltaRemaining;
        }
        // planet ids
        public int src_ { get; set; }
        public int dst_ { get; set; }
        public Owner owner_ { get; set; }
        public int population_ { get; set; }
        public int totalTurns_ { get; set; }
        public int turnsRemaining_ { get; set; }
    }
}
