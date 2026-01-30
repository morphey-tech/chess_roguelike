using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Project.Core.Core.Infrastructure
{
    /// <summary>
    /// Parses bracket notation format like "[P1][P2][.]" into array of aliases.
    /// Supports multi-character aliases inside brackets.
    /// </summary>
    public static class BracketNotationParser
    {
        private static readonly Regex BracketPattern = new(@"\[([^\]]*)\]", RegexOptions.Compiled);
        
        /// <summary>
        /// Parses a single row string like "[P1][.][P2]" into list of aliases ["P1", ".", "P2"]
        /// </summary>
        public static List<string> ParseRow(string row)
        {
            if (string.IsNullOrEmpty(row))
                return new List<string>();

            var result = new List<string>();
            var matches = BracketPattern.Matches(row);
            
            foreach (Match match in matches)
            {
                result.Add(match.Groups[1].Value);
            }

            return result;
        }

        /// <summary>
        /// Parses multiple rows into 2D array of aliases.
        /// Rows are inverted so that visual top in config = top of the board in game.
        /// </summary>
        public static string[,] ParseRows(string[] rows, int expectedWidth, int expectedHeight)
        {
            if (rows == null || rows.Length == 0)
                return new string[0, 0];

            var result = new string[expectedHeight, expectedWidth];
            
            for (int row = 0; row < Math.Min(rows.Length, expectedHeight); row++)
            {
                var aliases = ParseRow(rows[row]);
                
                if (aliases.Count != expectedWidth)
                {
                    throw new Exception($"Row {row} has {aliases.Count} cells, expected {expectedWidth}. Row: '{rows[row]}'");
                }

                // Invert row index so that visual top in config = top of the board in game
                int gameRow = (expectedHeight - 1) - row;
                
                for (int col = 0; col < expectedWidth; col++)
                {
                    result[gameRow, col] = aliases[col];
                }
            }

            return result;
        }

        /// <summary>
        /// Parses rows without known dimensions - infers from content.
        /// </summary>
        public static (List<List<string>> data, int width, int height) ParseRowsAuto(string[] rows)
        {
            if (rows == null || rows.Length == 0)
                return (new List<List<string>>(), 0, 0);

            var result = new List<List<string>>();
            int maxWidth = 0;

            foreach (string row in rows)
            {
                var aliases = ParseRow(row);
                result.Add(aliases);
                maxWidth = Math.Max(maxWidth, aliases.Count);
            }

            return (result, maxWidth, result.Count);
        }

        /// <summary>
        /// Converts aliases list back to bracket notation string.
        /// </summary>
        public static string ToBracketNotation(IEnumerable<string> aliases)
        {
            return string.Join("", System.Linq.Enumerable.Select(aliases, a => $"[{a}]"));
        }
    }
}
