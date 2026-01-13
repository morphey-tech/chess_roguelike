using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Boards
{
    [Serializable]
    public class BoardConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("width")]
        public int Width { get; set; } = 8;
        
        [JsonProperty("height")]
        public int Height { get; set; } = 8;
        
        [JsonProperty("board_data")]
        public string[] Board { get; set; }

        [JsonProperty("appear_id")]
        public string AppearStrategyId { get; set; }
        
        public char[,] GetBoard2D()
        {
            char[,] board2D = new char[Height, Width];
            for (int i = 0; i < Height; i++)
            {
                string row = Board[i];
                if (row.Length != Width)
                {
                    throw new Exception($"Row {i} length {row.Length} does not match board width {Width}");
                }

                for (int j = 0; j < Width; j++)
                {
                    board2D[i, j] = row[j];
                }
            }

            return board2D;
        }
    }
}