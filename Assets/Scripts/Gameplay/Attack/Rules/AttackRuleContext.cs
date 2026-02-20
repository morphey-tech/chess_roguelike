using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Context for attack rule validation.
    /// </summary>
    public readonly struct AttackRuleContext
    {
        public Figure Attacker { get; }
        public Figure Target { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public BoardGrid Grid { get; }

        public AttackRuleContext(
            Figure attacker,
            Figure target,
            GridPosition from,
            GridPosition to,
            BoardGrid grid)
        {
            Attacker = attacker;
            Target = target;
            From = from;
            To = to;
            Grid = grid;
        }
    }
}
