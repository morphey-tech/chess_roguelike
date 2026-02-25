using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Components;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Manages prepare placement highlights with incremental updates.
    /// </summary>
    public sealed class PrepareHighlightService
    {
        public void BuildPlacementCache(PrepareContext context)
        {
            context.AvailablePlacementPositions.Clear();
            foreach (BoardCell cell in context.Grid.AllCells())
            {
                GridPosition pos = cell.Position;
                if (context.Rules.CanPlace(pos))
                {
                    context.AvailablePlacementPositions.Add(pos);
                }
            }
        }

        public void ApplyAll(PrepareContext context)
        {
            foreach (BoardCell cell in context.Grid.AllCells())
            {
                ApplyOne(context, cell.Position);
            }
        }

        public void ApplyDirty(PrepareContext context, IReadOnlyCollection<GridPosition>? dirtyPositions)
        {
            if (dirtyPositions == null || dirtyPositions.Count == 0)
            {
                return;
            }

            foreach (GridPosition pos in dirtyPositions)
            {
                if (!context.Grid.IsInside(pos))
                {
                    continue;
                }

                ApplyOne(context, pos);
            }
        }

        public void Clear(BoardGrid grid)
        {
            foreach (BoardCell cell in grid.AllCells())
            {
                cell.Del<HighlightTag>();
                cell.Del<AttackHighlightTag>();
            }
        }

        private static void ApplyOne(PrepareContext context, GridPosition pos)
        {
            BoardCell cell = context.Grid.GetBoardCell(pos);
            cell.Del<AttackHighlightTag>();

            if (context.AvailablePlacementPositions.Contains(pos))
            {
                cell.EnsureComponent(new HighlightTag());
            }
            else
            {
                cell.Del<HighlightTag>();
            }
        }
    }
}
