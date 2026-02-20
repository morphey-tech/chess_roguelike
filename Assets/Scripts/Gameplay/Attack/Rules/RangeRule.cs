namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Basic range rule - target must be in attack range.
    /// </summary>
    public sealed class RangeRule : IAttackRule
    {
        public int Priority => 0;

        public bool Validate(AttackRuleContext context)
        {
            var strategy = AttackStrategyFactory.Instance.Get(context.Attacker.AttackId);
            return strategy.CanAttack(context.Attacker, context.From, context.To, context.Grid);
        }
    }
}
