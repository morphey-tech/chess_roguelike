namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Temporary damage modifier: +percent% damage per allied neighbour (e.g. 5% per ally).
    /// Used by Swarm passive, applied at turn start and expired after duration.
    /// </summary>
    public class SwarmDamageModifier : ICombatStatModifier
    {
        public int Priority => 200;

        private readonly int _allies;
        private readonly float _percentPerAlly; // e.g. 5 = 5%

        public SwarmDamageModifier(int allies, float percentPerAlly)
        {
            _allies = allies;
            _percentPerAlly = percentPerAlly;
        }

        public void Modify(CombatStatContext ctx)
        {
            float multiplier = 1f + _allies * (_percentPerAlly / 100f);
            ctx.Damage = ctx.Damage * multiplier;
        }
    }
}