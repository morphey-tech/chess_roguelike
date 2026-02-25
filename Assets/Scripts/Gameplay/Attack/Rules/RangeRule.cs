using System.Linq;
using Project.Gameplay.Gameplay.Combat.Passives;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using VContainer;

namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Basic range rule - target must be in attack range.
    /// Skips validation if Desperation passive is active (handled by DesperationRule).
    /// </summary>
    public sealed class RangeRule : IAttackRule
    {
        private readonly AttackStrategyFactory _strategyFactory;

        public int Priority => 0;

        [Inject]
        private RangeRule(AttackStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        public bool Validate(AttackRuleContext context)
        {
            Figure attacker = context.Attacker;
            if (attacker.BasePassives.Any(p => p is DesperationPassive))
            {
                BoardGrid grid = context.Grid;
                int allies = grid.CountAlliesAround(attacker);
                if (allies == 0)
                {
                    return true; // Desperation allows any adjacent target
                }
            }
            IAttackStrategy strategy = _strategyFactory.Get(context.Attacker.AttackId);
            return strategy.CanAttack(context.Attacker, context.From, context.To, context.Grid);
        }
    }
}
