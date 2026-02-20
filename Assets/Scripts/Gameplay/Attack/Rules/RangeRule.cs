using System.Linq;
using Project.Gameplay.Gameplay.Combat.Passives;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Basic range rule - target must be in attack range.
    /// Skips validation if Desperation passive is active (handled by DesperationRule).
    /// </summary>
    public sealed class RangeRule : IAttackRule
    {
        public int Priority => 0;

        public bool Validate(AttackRuleContext context)
        {
            Figure attacker = context.Attacker;
            if (attacker.BasePassives.Any(p => p is DesperationPassive))
            {
                BoardGrid grid = context.Grid;
                int allies = grid.CountAlliesAround(attacker);
                if (allies == 0)
                    return true; // Desperation allows any adjacent target
            }
            IAttackStrategy strategy = AttackStrategyFactory.Instance.Get(context.Attacker.AttackId);
            return strategy.CanAttack(context.Attacker, context.From, context.To, context.Grid);
        }
    }
}
