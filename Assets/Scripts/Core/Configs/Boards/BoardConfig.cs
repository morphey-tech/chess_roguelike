using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs
{
    [Serializable]
    public class BoardConfig
    {
        [JsonProperty("width")]
        public int Width { get; set; } = 8;
        
        [JsonProperty("height")]
        public int Height { get; set; } = 8;
        
        [JsonProperty("board_data")]
        public string[] Board { get; set; }
        
        public char[,] GetBoard2D()
        {
            char[,] board2D = new char[Height, Width];
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    board2D[i, j] = Board[i * Width + j][0]; // Конвертируем строку в символ
                }
            }
            return board2D;
        }
    }
}