using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Physics;
using Project.Gameplay.Presentations;
using UnityEngine;

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
        
        public static Vector3 GetCellTopPosition(GridPosition gridPos)
        {
            const float surfaceY = 0f;

            Vector3 rayOrigin = new(gridPos.Column, PhysicsSettings.CellRaycastHeight, gridPos.Row);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit,
                    PhysicsSettings.CellRaycastHeight * 2f, PhysicsSettings.CellLayerMask))
            {
                return hit.point;
            }

            return new Vector3(gridPos.Column, surfaceY, gridPos.Row);
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

        public IEnumerable<Figures.Figure> GetAllFigures()
        {
            foreach (BoardCell? cell in AllCells())
            {
                if (cell.OccupiedBy != null)
                    yield return cell.OccupiedBy;
            }
        }
        


        public IEnumerable<Figures.Figure> GetFiguresByTeam(Figures.Team team)
        {
            foreach (BoardCell? cell in AllCells())
            {
                if (cell.OccupiedBy?.Team == team)
                    yield return cell.OccupiedBy;
            }
        }
    }
}