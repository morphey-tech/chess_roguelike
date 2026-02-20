namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Disarm rule - disarmed attackers cannot attack.
    /// </summary>
    public sealed class DisarmRule : IAttackRule
    {
        public int Priority => 10;

        public bool Validate(AttackRuleContext context)
        {
            // TODO: Implement disarm status effect check
            // return !context.Attacker.Effects.HasEffect<DisarmEffect>();
            return true;
        }
    }
}
