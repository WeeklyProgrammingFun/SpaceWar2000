using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BotBase
{
    public enum Owner
    {
        None,
        Player1, 
        Player2
    }
    public class Planet
    {
        public double x_, y_;
        public Owner owner_;
        public int id_;
        public int population_;
        public int growthRate_;
    }

    public class Fleet
    {
        public Owner owner_;
        public int src_, dst_;
        public int population_, remainingTurns_;
    }

    public class Command
    {
        public int src_, dst_, pop_;
    }


    public abstract class BotBase
    {
        protected List<Fleet> Fleets = new List<Fleet>();
        protected List<Planet> Planets = new List<Planet>();

        protected abstract void GenMoves();

        protected List<Planet> MyPlanets()
        {
            return Planets.Where(p => p.owner_ == Owner.Player1).ToList();

        }
        protected List<Planet> EnemyPlanets()
        {
            return Planets.Where(p => p.owner_ == Owner.Player2).ToList();
        }
        protected List<Planet> NeutralPlanets()
        {
            return Planets.Where(p => p.owner_ == Owner.None).ToList();
        }

        public int Distance(Planet a, Planet b)
        {
            var dx = a.x_ - b.x_;
            var dy = a.y_ - b.y_;
            var d = Math.Sqrt(dx * dx + dy * dy);
            return (int) Math.Ceiling(d);
        }

        public List<Command> Commands = new List<Command>();

        protected void LaunchFleet(int src, int dst, int population)
        {
            if (src != dst) // ignore these just in case
                Commands.Add(new Command{src_ = src, dst_ = dst, pop_ = population});
        }

        protected int Frame=0,MaxFrames=200;
        protected string EnemyName = "Bob";

        protected Random Rand { get; set;  }

        void StartGame(string line)
        {
            Frame = 0;
            var tokens = line.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length > 1)
                EnemyName = tokens[1];
            else
                EnemyName = "Bob";
            if (tokens.Length > 2 || !Int32.TryParse(tokens[2],out MaxFrames))
            {
                
            }
            else
            {
                MaxFrames = 200;
            }
            Rand = new Random(1234); // todo - seed from input
        }

        void Error(string error)
        {
            // todo - console? error?
        }

        void ParseState(string line)
        { // form STATE planets, fleets, E
            Commands.Clear();
            Planets.Clear();
            Fleets.Clear();

            var tokens = line.Split(new[] { ' ','\r','\n','\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2)
                return;

            int index = 1;
            int planetIndex = 0;
            while (index < tokens.Length)
            {
                if (tokens[index] == "P" && index + 5 < tokens.Length)
                {
                    if (
                        Double.TryParse(tokens[index + 1], out var x) &&
                        Double.TryParse(tokens[index + 2], out var y) &&
                        Int32.TryParse(tokens[index + 3], out var owner) &&
                        Int32.TryParse(tokens[index + 4], out var population) &&
                        Int32.TryParse(tokens[index + 5], out var growthRate)
                    )
                    {
                        Planets.Add(new Planet
                        {
                            x_ = x, y_ = y,
                            owner_ = (Owner)owner,
                            growthRate_ = growthRate,
                            population_ = population,
                            id_ = planetIndex
                        });
                        index += 6;
                    }
                    else
                    {
                        Error("Planet parse error in " + line);
                        index++;
                    }

                    planetIndex++;
                }
                else if (tokens[index] == "F")
                {
                    if (
                        Int32.TryParse(tokens[index + 1], out var owner) &&
                        Int32.TryParse(tokens[index + 2], out var population) &&
                        Int32.TryParse(tokens[index + 3], out var src) &&
                        Int32.TryParse(tokens[index + 4], out var dst) &&
                        Int32.TryParse(tokens[index + 5], out var remainingTurns)
                    )
                    {
                        Fleets.Add(new Fleet
                        {
                            owner_ = (Owner)owner,
                            src_ = src, 
                            dst_ = dst,
                            population_ = population,
                            remainingTurns_ = remainingTurns

                        });
                        index += 6;
                    }
                    else
                    {
                        Error("Planet parse error in " + line);
                        index++;
                    }

                }
                else
                {
                    Error("Parse error in " + line);
                    ++index;
                }
            }
        }

        string FormatCommands()
        {
            var sb = new StringBuilder();
            sb.Append("MOVE ");
            foreach (var c in Commands)
                sb.Append($"L {c.src_} {c.dst_} {c.pop_} ");
            sb.Append("E");
            return sb.ToString();
        }

        // game loop, process console commands
        public void Run(string[] args)
        {
            var done = false;
            while (!done)
            {
                var line = "";
                while (!line.EndsWith(" E"))
                {
                    var temp = Console.ReadLine();
                    line += temp;
                }

                Debug.WriteLine("Bot received " + line);
                if (line.StartsWith("START"))
                    StartGame(line);
                else if (line.StartsWith("STATE"))
                {
                    ParseState(line);
                    GenMoves();
                    ++Frame;
                    var launchText = FormatCommands();
                    Debug.WriteLine("Bot writing " + launchText);
                    Console.WriteLine(launchText);
                }
                else if (line.StartsWith("RESULT"))
                {

                }
                else if (line.StartsWith("QUIT"))
                {
                    Console.Beep(440,1);
                    done = true;
                }
                else
                {
                    // error - not understood
                }
            }
        }
    }
}
