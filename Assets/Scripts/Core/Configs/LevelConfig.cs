namespace Project.Core.Core.Configs
{
    public class LevelConfig
    {
        public int Width { get; set; } = 8;
        public int Height { get; set; } = 8;
        
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