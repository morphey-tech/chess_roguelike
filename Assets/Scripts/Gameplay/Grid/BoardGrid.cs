using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Grid
{
    public sealed class BoardGrid : IBoardGrid
    {
        private readonly BoardCell[,] _cells;

        public int Width { get; }
        public int Height { get; }

        public BoardGrid(int width, int height)
        {
            Width = width;
            Height = height;

            _cells = new BoardCell[height, width];

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    _cells[r, c] =
                        new BoardCell(new GridPosition(r, c));
                }
            }
        }

        public bool IsInside(GridPosition position)
        {
            return position.Row >= 0 &&
                   position.Row < Height &&
                   position.Column >= 0 &&
                   position.Column < Width;
        }

        public IGridCell GetCell(GridPosition position)
        {
            return GetBoardCell(position);
        }

        public BoardCell GetBoardCell(GridPosition position)
        {
            return _cells[position.Row, position.Column];
        }
    }
}