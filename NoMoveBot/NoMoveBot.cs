using BotBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoMoveBot
{
    class NoMoveBot : BotBase.BotBase
    {
        protected override void GenMoves()
        {
            // do nothing
        }

        static void Main(string[] args)
        {
            new NoMoveBot().Run(args);
        }
    }
}
