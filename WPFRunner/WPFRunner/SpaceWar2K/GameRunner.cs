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

        public static Result PlayOneGame(Player p1, Player p2, int randomSeed, Action<State> stateAction)
        {
            int maxMs = 5000;
            var gen = new MapGenerator();

            var state = new State(1, 200, gen.Make(randomSeed), new List<Fleet>());
            stateAction?.Invoke(state);

            p1.Write($"START {Path.GetFileNameWithoutExtension(p2.Filename)} {randomSeed} E");
            p2.Write($"START {Path.GetFileNameWithoutExtension(p1.Filename)} {randomSeed} E");

            while (true)
            {
                p1.Write("STATE " + Formatter.StateToText(state,false) +" E");
                p2.Write("STATE " + Formatter.StateToText(state, true) + " E");

                var move1 = ReadMove(p1,maxMs);
                var move2 = ReadMove(p2, maxMs);

                if (!move1.EndsWith(" E"))
                    return (Result.Player2Win);
                if (!move2.EndsWith(" E"))
                    return (Result.Player1Win);

                var l1 = Formatter.MoveToLaunches(move1, Owner.Player1);
                var l2 = Formatter.MoveToLaunches(move2, Owner.Player2);

                if (l1 == null)
                    return (Result.Player2Win); // illegal move
                if (l2 == null)
                    return (Result.Player1Win); // illegal move

                var launches = l1.Concat(l2).ToList();
                var (result, nextState) = StateUpdater.Update(launches, state);

                state = nextState;
                stateAction?.Invoke(state);

                if (result != Result.Unfinished)
                {
                    if (result == Result.Player1Win)
                    {
                        p1.Write("RESULT Win E");
                        p2.Write("RESULT Lose E");
                    }
                    else if (result == Result.Player2Win)
                    {
                        p1.Write("RESULT Lose E");
                        p2.Write("RESULT Win E");
                    }
                    else if (result == Result.Tie)
                    {
                        p1.Write("RESULT Tie E");
                        p2.Write("RESULT Tie E");
                    }
                    return (result);
                }
            }
        }
    }
}
