namespace Project.Core.Core.Combat.Contexts
{
    /// <summary>
    /// Lightweight damage context for trigger system.
    /// Uses object references to avoid dependency on Figure.
    /// </summary>
    public sealed class DamageContext
    {
        /// <summary>
        /// The attacker (Figure in gameplay layer).
        /// </summary>
        public object Attacker { get; set; } = null!;

        /// <summary>
        /// The target (Figure in gameplay layer).
        /// </summary>
        public object Target { get; set; } = null!;

        /// <summary>
        /// Base damage before modifiers.
        /// </summary>
        public int BaseDamage { get; set; }

        /// <summary>
        /// Damage multiplier (crit, buffs, etc.).
        /// </summary>
        public float DamageMultiplier { get; set; } = 1f;

        /// <summary>
        /// Flat bonus damage added to base.
        /// </summary>
        public int BonusDamage { get; set; }

        /// <summary>
        /// Is this hit a critical?
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Was this hit dodged?
        /// </summary>
        public bool IsDodged { get; set; }

        /// <summary>
        /// Should this hit be cancelled?
        /// </summary>
        public bool IsCancelled { get; set; }
    }

    /// <summary>
    /// Context for kill events.
    /// </summary>
    public sealed class KillContext
    {
        /// <summary>
        /// The figure that dealt the killing blow.
        /// </summary>
        public object Killer { get; set; } = null!;

        /// <summary>
        /// The figure that was killed.
        /// </summary>
        public object Victim { get; set; } = null!;
    }

    /// <summary>
    /// Context for death events.
    /// </summary>
    public sealed class DeathContext
    {
        /// <summary>
        /// The figure that died.
        /// </summary>
        public object Victim { get; set; } = null!;

        /// <summary>
        /// The figure that caused the death (if any).
        /// </summary>
        public object Killer { get; set; } = null!;
    }

    /// <summary>
    /// Context for movement events.
    /// </summary>
    public sealed class MoveContext
    {
        /// <summary>
        /// The figure that moved.
        /// </summary>
        public object Actor { get; set; } = null!;

        /// <summary>
        /// Starting X position.
        /// </summary>
        public int FromX { get; set; }

        /// <summary>
        /// Starting Y position.
        /// </summary>
        public int FromY { get; set; }

        /// <summary>
        /// Destination X position.
        /// </summary>
        public int ToX { get; set; }

        /// <summary>
        /// Destination Y position.
        /// </summary>
        public int ToY { get; set; }

        /// <summary>
        /// Current turn number.
        /// </summary>
        public int CurrentTurn { get; set; }
    }

    /// <summary>
    /// Context for turn events.
    /// </summary>
    public sealed class TurnContext
    {
        /// <summary>
        /// Current turn number.
        /// </summary>
        public int CurrentTurn { get; set; }

        /// <summary>
        /// Team ID (0 = player, 1 = enemy, etc.).
        /// </summary>
        public int TeamId { get; set; }
    }
}
