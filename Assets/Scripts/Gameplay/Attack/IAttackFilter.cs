using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    public sealed class AttackContext
    {
        public Figure Attacker { get; }
        public GridPosition From { get; }
        public BoardGrid Grid { get; }

        public AttackContext(Figure attacker, GridPosition from, BoardGrid grid)
        {
            Attacker = attacker;
            From = from;
            Grid = grid;
        }
    }

    public interface IAttackFilter
    {
        void FilterTargets(List<GridPosition> targets, AttackContext context);
    }

    public interface IAttackRangeModifier
    {
        bool CanAttackCell(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid);
    }
}
