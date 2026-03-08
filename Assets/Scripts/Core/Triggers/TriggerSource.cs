namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Source of a trigger event. Used to prevent infinite loops and filter triggers.
    /// </summary>
    public enum TriggerSource
    {
        /// <summary>
        /// Unknown or default source.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Combat action (attack, damage, etc.).
        /// </summary>
        Combat = 1,

        /// <summary>
        /// Artifact effect.
        /// </summary>
        Artifact = 2,

        /// <summary>
        /// Passive ability.
        /// </summary>
        Passive = 3,

        /// <summary>
        /// Status effect (buff, debuff).
        /// </summary>
        StatusEffect = 4,

        /// <summary>
        /// Environmental effect (terrain, weather).
        /// </summary>
        Environment = 5,

        /// <summary>
        /// Direct damage (DoT, reflection).
        /// </summary>
        DirectDamage = 6,

        /// <summary>
        /// Heal effect.
        /// </summary>
        Heal = 7,

        /// <summary>
        /// Custom source (use SourceObject for details).
        /// </summary>
        Custom = 100
    }
}