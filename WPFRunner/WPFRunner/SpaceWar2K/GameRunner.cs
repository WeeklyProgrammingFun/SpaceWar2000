using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFRunner.Model;

namespace WPFRunner.SpaceWar2K
{
    public static class GameRunner
    {
        /*
 * Game loop:
 * 1. send state to bots
 * 2. Receive orders
 * 3. Update state
 * 4. Check end game
 */

        static string ReadMove(Player player, int maxMs)
        {
            var move = "";
            var curMs = Environment.TickCount;
            while (!move.EndsWith(" E") && Environment.TickCount <= curMs + maxMs)
            {
                var temp = player.Read(maxMs).Aggregate("", (a, b) => a + b);
                move += temp;
            }
            return move;
        }

        public static Result PlayOneGame(Player p1, Player p2, int randomSeed, Action<FrameInfo> frameAction)
        {
            int maxMs = 5000;
            var gen = new MapGenerator();

            var state = new State(1, 200, gen.Make(randomSeed), new List<Fleet>());
            var fInfo = new FrameInfo{State = state};
            
            Write(p1,$"START {Path.GetFileNameWithoutExtension(p2.Filename)} {randomSeed} E");
            Write(p2,$"START {Path.GetFileNameWithoutExtension(p1.Filename)} {randomSeed} E");
            frameAction?.Invoke(fInfo);

            var result = Result.Unfinished;
            while (result == Result.Unfinished)
            {
                fInfo = new FrameInfo{State = state};

                Write(p1,"STATE " + Formatter.StateToText(state,false) + " E");
                Write(p2,"STATE " + Formatter.StateToText(state, true) + " E");

                var move1 = Read(p1);
                var move2 = Read(p2);

                var l1 = Formatter.MoveToLaunches(move1, Owner.Player1);
                var l2 = Formatter.MoveToLaunches(move2, Owner.Player2);

                if (l1 == null && l2 != null)
                {
                    fInfo.Error = "Player 1 invalid move";
                    result = Result.Player2Win;
                }
                else if (l2 == null && l1 != null)
                {
                    fInfo.Error = "Player 2 invalid move";
                    result = Result.Player1Win;
                }
                else if (l1 == null && l2 == null)
                {
                    fInfo.Error = "Both players invalid move";
                    result = Result.Tie;
                }
                else
                {
                    var launches = l1.Concat(l2).ToList();
                    var (result3, nextState) = StateUpdater.Update(launches, state);
                    result = result3;
                    state = nextState;
                }
                frameAction?.Invoke(fInfo);
            }

            fInfo = new FrameInfo {State = state};
            if (result != Result.Unfinished)
            {
                if (result == Result.Player1Win)
                {
                    Write(p1,"RESULT Win E");
                    Write(p2,"RESULT Lose E");
                }
                else if (result == Result.Player2Win)
                {
                    Write(p1,"RESULT Lose E");
                    Write(p2,"RESULT Win E");
                }
                else if (result == Result.Tie)
                {
                    Write(p1,"RESULT Tie E");
                    Write(p2,"RESULT Tie E");
                }
            }

            frameAction?.Invoke(fInfo);

            return result;

            // write and log to frame
            void Write(Player p, string msg)
            {
                p.Write(msg);
                fInfo.Write(p.Filename, msg);
            }

            // read from player, log to frame
            string Read(Player p)
            {
                var move = ReadMove(p, maxMs);
                fInfo.Read(p.Filename, move);
                return move;
            }

        }
    }
}
