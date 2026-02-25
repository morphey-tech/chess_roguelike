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
        
        public static List<string> ParseRow(string row)
        {
            if (string.IsNullOrEmpty(row))
            {
                return new List<string>();
            }

            List<string> result = new List<string>();
            MatchCollection matches = BracketPattern.Matches(row);
            
            foreach (Match match in matches)
            {
                result.Add(match.Groups[1].Value);
            }

            return result;
        }

        public static string[,] ParseRows(string[]? rows, int expectedWidth, int expectedHeight)
        {
            if (rows == null || rows.Length == 0)
            {
                return new string[0, 0];
            }

            string[,] result = new string[expectedHeight, expectedWidth];
            
            for (int row = 0; row < Math.Min(rows.Length, expectedHeight); row++)
            {
                List<string> aliases = ParseRow(rows[row]);
                
                if (aliases.Count != expectedWidth)
                {
                    throw new Exception($"Row {row} has {aliases.Count} cells, expected {expectedWidth}. Row: '{rows[row]}'");
                }

                int gameRow = (expectedHeight - 1) - row;
                for (int col = 0; col < expectedWidth; col++)
                {
                    result[gameRow, col] = aliases[col];
                }
            }

            return result;
        }

        public static (List<List<string>> data, int width, int height) ParseRowsAuto(string[]? rows)
        {
            if (rows == null || rows.Length == 0)
            {
                return (new List<List<string>>(), 0, 0);
            }

            List<List<string>> result = new List<List<string>>();
            int maxWidth = 0;

            foreach (string row in rows)
            {
                List<string> aliases = ParseRow(row);
                result.Add(aliases);
                maxWidth = Math.Max(maxWidth, aliases.Count);
            }

            return (result, maxWidth, result.Count);
        }

        public static string ToBracketNotation(IEnumerable<string> aliases)
        {
            return string.Join("", System.Linq.Enumerable.Select(aliases, a => $"[{a}]"));
        }
    }
}
