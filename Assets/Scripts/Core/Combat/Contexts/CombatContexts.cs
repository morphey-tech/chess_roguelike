using Project.Core.Core.Combat;
using Project.Core.Core.Grid;

namespace Project.Core.Core.Combat.Contexts
{
    /// <summary>
    /// Base interface for damage-related contexts.
    /// Core layer uses object references to avoid dependency on Figure.
    /// </summary>
    public interface IDamageContext
    {
        /// <summary>
        /// The attacker (Figure in gameplay layer).
        /// </summary>
        object Attacker { get; }

        /// <summary>
        /// The target (Figure in gameplay layer).
        /// </summary>
        object Target { get; }

        /// <summary>
        /// Base damage before modifiers.
        /// </summary>
        float BaseDamage { get; }

        /// <summary>
        /// Damage multiplier (crit, buffs, etc.).
        /// </summary>
        float DamageMultiplier { get; set; }

        /// <summary>
        /// Flat bonus damage added to base.
        /// </summary>
        float BonusDamage { get; set; }

        /// <summary>
        /// Is this hit a critical?
        /// </summary>
        bool IsCritical { get; set; }

        /// <summary>
        /// Was this hit dodged?
        /// </summary>
        bool IsDodged { get; set; }

        /// <summary>
        /// Should this hit be cancelled?
        /// </summary>
        bool IsCancelled { get; set; }
    }

    /// <summary>
    /// Mutable damage context for Core layer operations.
    /// </summary>
    public sealed class DamageContext : IDamageContext
    {
        public object Attacker { get; set; } = null!;
        public object Target { get; set; } = null!;
        public float BaseDamage { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
        public float BonusDamage { get; set; }
        public bool IsCritical { get; set; }
        public bool IsDodged { get; set; }
        public bool IsCancelled { get; set; }
    }

    /// <summary>
    /// Context for kill events.
    /// </summary>
    public interface IKillContext
    {
        /// <summary>
        /// The figure that dealt the killing blow.
        /// </summary>
        object Killer { get; }

        /// <summary>
        /// The figure that was killed.
        /// </summary>
        object Victim { get; }
    }

    /// <summary>
    /// Context for death events.
    /// </summary>
    public interface IDeathContext
    {
        /// <summary>
        /// The figure that died.
        /// </summary>
        object Victim { get; }

        /// <summary>
        /// The figure that caused the death (if any).
        /// </summary>
        object? Killer { get; }
    }

    /// <summary>
    /// Context for movement events.
    /// </summary>
    public interface IMoveContext
    {
        /// <summary>
        /// The figure that moved.
        /// </summary>
        object Actor { get; }

        /// <summary>
        /// Starting position.
        /// </summary>
        GridPosition From { get; }

        /// <summary>
        /// Destination position.
        /// </summary>
        GridPosition To { get; }

        /// <summary>
        /// Did the figure actually move?
        /// </summary>
        bool DidMove { get; }
    }

    /// <summary>
    /// Context for turn events.
    /// </summary>
    public interface ITurnContext
    {
        /// <summary>
        /// Current turn number.
        /// </summary>
        int TurnNumber { get; }

        /// <summary>
        /// Team whose turn it is.
        /// </summary>
        Team Team { get; }
    }
}
