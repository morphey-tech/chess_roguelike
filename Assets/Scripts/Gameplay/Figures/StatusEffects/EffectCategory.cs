namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    /// <summary>
    /// Category of status effect for filtering and interactions.
    /// </summary>
    public enum EffectCategory
    {
        /// <summary>
        /// No specific category.
        /// </summary>
        None = 0,

        /// <summary>
        /// Positive effects (buffs, shields, regeneration).
        /// </summary>
        Buff = 1,

        /// <summary>
        /// Negative effects (poison, burn, vulnerability).
        /// </summary>
        Debuff = 2,

        /// <summary>
        /// Neutral effects (marks, triggers, auras).
        /// </summary>
        Neutral = 3,

        /// <summary>
        /// Crowd control effects (stun, silence, root, disarm).
        /// </summary>
        CrowdControl = 4,
    }
}
