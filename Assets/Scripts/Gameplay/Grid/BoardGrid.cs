using System.Collections.Generic;
using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Presentations;

namespace Project.Gameplay.Gameplay.Grid
{
    public sealed class BoardGrid : IBoardGrid
    {
        private static readonly (int dr, int dc)[] AdjacentOffsets =
        {
            (-1, -1), (-1, 0), (-1, 1),
            (0, -1),           (0, 1),
            (1, -1),  (1, 0),  (1, 1)
        };

        private readonly BoardCell[,] _cells;
        private readonly Dictionary<int, BoardCell> _figureLookup;

        public int Width { get; }
        public int Height { get; }

        public BoardGrid(int width, int height)
        {
            Width = width;
            Height = height;

            _cells = new BoardCell[height, width];
            _figureLookup = new Dictionary<int, BoardCell>();

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    _cells[r, c] = new BoardCell(IdGetter.MakeId(), new GridPosition(r, c));
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

        public IEnumerable<BoardCell> AllCells()
        {
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    yield return _cells[r, c];
                }
            }
        }

        public BoardCell? FindFigure(Figures.Figure figure)
        {
            foreach (BoardCell? cell in AllCells())
            {
                if (cell.OccupiedBy == figure)
                    return cell;
            }
            return null;
        }

        public IEnumerable<Figure> GetAllFigures()
        {
            foreach (BoardCell? cell in AllCells())
            {
                if (cell.OccupiedBy != null)
                    yield return cell.OccupiedBy;
            }
        }

        public Figure? GetFigureById(int id)
        {
            if (_figureLookup.TryGetValue(id, out BoardCell? cell))
            {
                return cell.OccupiedBy;
            }
            return null;
        }

        public void PlaceFigure(Figure figure, GridPosition position)
        {
            var cell = GetBoardCell(position);
            _figureLookup[figure.Id] = cell;
            cell.SetOccupant(figure);
        }

        public void RemoveFigure(Figure figure)
        {
            var cell = FindFigure(figure);
            if (cell != null)
            {
                _figureLookup.Remove(figure.Id);
                cell.SetOccupant(null);
            }
        }

        public IEnumerable<Figure> GetFiguresByTeam(Team team)
        {
            foreach (BoardCell? cell in AllCells())
            {
                if (cell.OccupiedBy?.Team == team)
                    yield return cell.OccupiedBy;
            }
        }

        /// <summary>
        /// Returns all valid adjacent cells around a position (8 directions).
        /// </summary>
        public IEnumerable<BoardCell> GetAdjacentCells(GridPosition position)
        {
            foreach ((int dr, int dc) in AdjacentOffsets)
            {
                GridPosition neighbor = new(position.Row + dr, position.Column + dc);
                if (IsInside(neighbor))
                {
                    yield return GetBoardCell(neighbor);
                }
            }
        }
    }
}