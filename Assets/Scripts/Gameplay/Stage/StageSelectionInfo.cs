using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Stage
{
    public sealed class StageSelectionInfo
    {
        public IReadOnlyCollection<GridPosition> MoveTargets { get; }
        public IReadOnlyCollection<GridPosition> AttackTargets { get; }

        public StageSelectionInfo(
            IReadOnlyCollection<GridPosition> moveTargets,
            IReadOnlyCollection<GridPosition> attackTargets)
        {
            MoveTargets = moveTargets ?? new HashSet<GridPosition>();
            AttackTargets = attackTargets ?? new HashSet<GridPosition>();
        }

        public static StageSelectionInfo ForMoves(IReadOnlyCollection<GridPosition> moves)
        {
            return new StageSelectionInfo(moves, Array.Empty<GridPosition>());
        }

        public static StageSelectionInfo ForCombat(
            IReadOnlyCollection<GridPosition> moves,
            IReadOnlyCollection<GridPosition> attacks)
        {
            return new StageSelectionInfo(moves, attacks);
        }
    }
}
