using System.Collections.Generic;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Stage.Analysis
{
    /// <summary>
    /// Анализ тактических возможностей конкретной фигуры.
    /// </summary>
    public sealed class StageActorAnalysis
    {
        public IReadOnlyCollection<GridPosition> MoveTargets { get; }
        public IReadOnlyCollection<GridPosition> AttackTargets { get; }
        public IReadOnlyCollection<GridPosition> DangerousCells { get; }

        public StageActorAnalysis(
            IReadOnlyCollection<GridPosition> moveTargets,
            IReadOnlyCollection<GridPosition> attackTargets,
            IReadOnlyCollection<GridPosition> dangerousCells)
        {
            MoveTargets = moveTargets;
            AttackTargets = attackTargets;
            DangerousCells = dangerousCells;
        }
    }
}
