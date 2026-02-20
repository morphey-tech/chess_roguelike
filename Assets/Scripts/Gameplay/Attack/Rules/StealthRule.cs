namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Stealth rule - stealthed targets cannot be attacked.
    /// </summary>
    public sealed class StealthRule : IAttackRule
    {
        public int Priority => 50;

        public bool Validate(AttackRuleContext context)
        {
            // TODO: Implement stealth status effect check
            // return !context.Target.Effects.HasEffect<StealthEffect>();
            return true;
        }
    }
}
