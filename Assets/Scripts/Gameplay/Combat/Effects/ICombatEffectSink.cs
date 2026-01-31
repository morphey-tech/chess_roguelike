namespace Project.Gameplay.Gameplay.Combat.Effects
{
    /// <summary>
    /// Unified interface for adding combat effects.
    /// Implemented by HitContext, AfterHitContext, and CombatEffectContext.
    /// </summary>
    public interface ICombatEffectSink
    {
        void AddEffect(ICombatEffect effect);
    }
}
