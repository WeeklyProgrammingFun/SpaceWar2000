using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFRunner.SpaceWar2K
{
    // formatting and parsing
    public static class Formatter
    {
        public static string StateToText(State state, bool swapPlayers)
        {
            int ToOwner(Owner owner)
            {
                if (owner == Owner.Player1) return swapPlayers ? 2 : 1;
                if (owner == Owner.Player2) return swapPlayers ? 1 : 2;
                return 0;
            }
            var sb = new StringBuilder();
            foreach (var p in state.planets_)
                sb.Append($"P {p.x_} {p.y_} {ToOwner(p.owner_)} {p.population_} {p.growthRate_} \n");
            foreach (var f in state.fleets_)
                sb.Append($"F {ToOwner(f.owner_)} {f.population_} {f.src_} {f.dst_} {f.totalTurns_} {f.turnsRemaining_} \n");
            return sb.ToString();
        }
        public static List<Launch> MoveToLaunches(string move, Owner owner)
        {   // MOVE [L src dst pop]* E
            // L src dst number ... E
            var clean = move.Replace("\r", " ").Replace("\n", " ");
            var tokens = clean.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (((tokens.Length - 2) % 4) != 0 || tokens.Last().ToUpper() != "E" || tokens.First().ToUpper() != "MOVE")
                return null; // cannot be valid
            var launches = new List<Launch>();
            var index = 1;
            while (index < tokens.Length-1)
            {
                if (tokens[index] != "L" ||
                    !Int32.TryParse(tokens[index + 1], out var src) ||
                    !Int32.TryParse(tokens[index + 2], out var dst) ||
                    !Int32.TryParse(tokens[index + 3], out var pop)
                    )
                    return null;
                launches.Add(new Launch { src_ = src, dst_ = dst, population_ = pop, owner_ = owner });
                index += 4;
            }
            return launches;
        }
    }
}
