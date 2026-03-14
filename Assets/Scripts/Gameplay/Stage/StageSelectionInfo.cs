using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Stage
{
    public sealed class StageSelectionInfo
    {
        public IReadOnlyCollection<GridPosition> MoveTargets { get; }
        public IReadOnlyCollection<GridPosition> AttackTargets { get; }
        public IReadOnlyCollection<GridPosition> UnderAttackTargets { get; }
        public GridPosition? CurrentPosition { get; }

        public StageSelectionInfo(
            IReadOnlyCollection<GridPosition> moveTargets,
            IReadOnlyCollection<GridPosition> attackTargets,
            IReadOnlyCollection<GridPosition> underAttackTargets = null,
            GridPosition? currentPosition = null)
        {
            MoveTargets = moveTargets ?? new HashSet<GridPosition>();
            AttackTargets = attackTargets ?? new HashSet<GridPosition>();
            UnderAttackTargets = underAttackTargets ?? new HashSet<GridPosition>();
            CurrentPosition = currentPosition;
        }

        public static StageSelectionInfo ForMoves(IReadOnlyCollection<GridPosition> moves)
        {
            return new StageSelectionInfo(moves, Array.Empty<GridPosition>());
        }

        public static StageSelectionInfo ForCombat(
            IReadOnlyCollection<GridPosition> moves,
            IReadOnlyCollection<GridPosition> attacks,
            IReadOnlyCollection<GridPosition> underAttacks = null,
            GridPosition? currentPosition = null)
        {
            return new StageSelectionInfo(moves, attacks, underAttacks, currentPosition);
        }
    }
}
