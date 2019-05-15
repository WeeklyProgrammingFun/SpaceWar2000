using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFRunner.Model;

namespace WPFRunner.SpaceWar2K
{
    class Interface
    {

        public static HeadToHeadScore PlayMatch(int rounds, Player player1, Player player2, int maxFrame)
        {
            var scorer = new HeadToHeadScore(player1, player2);
            for (var i = 0; i < rounds; ++i)
            {
                var res = GameRunner.PlayOneGame(player1, player2, 1234, null, maxFrame);
                scorer.Tally(player1, player2, res);
            }

            return scorer;
        }
    }
}
