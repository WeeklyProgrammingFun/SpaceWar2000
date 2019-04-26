using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFRunner.Model
{
    /// <summary>
    /// Run a tournament, given a cross table for the players
    /// </summary>
    class Runner
    {
        public class RunProgress
        {
            public double RatioCompleted;
            public HeadToHeadScore HeadToHeadScore;
        }

        CrossTable table;

        // Run the tournament async
        public Task RunAsync(int rounds, CrossTable crossTable, CancellationToken cancellationToken,
            IProgress<RunProgress> progress)
        {
            return Task.Run(() =>
                {
                    int completedMatches = 0;
                    int num = crossTable.Players.Count;
                    crossTable.Scores.Clear();
                    int numGames = num * (num - 1) / 2;
                    table = crossTable;
                    completedMatches = 0;
                    var roundMatchups = crossTable.NextRoundSchedule();

                    while (roundMatchups != null)
                    {

                        Parallel.For(0, roundMatchups.Count, n =>
                        {
                            var match = roundMatchups[n];
                            cancellationToken.ThrowIfCancellationRequested();
                            var player1 = match.Item1;
                            var player2 = match.Item2;
                            if (player1 != null && player2 != null)
                            {
                                // check for bye
                                var headTohead = SpaceWar2K.Interface.PlayMatch(rounds, player1, player2);
                                Interlocked.Increment(ref completedMatches);
                                double completedRatio = completedMatches;
                                completedRatio /= numGames;
                                progress.Report(new RunProgress
                                    {RatioCompleted = completedRatio, HeadToHeadScore = headTohead});
                            }
                        });
                        roundMatchups = crossTable.NextRoundSchedule();
                    }
                }
            );
        }
    }
}