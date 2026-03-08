using Project.Core.Core.Combat.Contexts;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    /// <summary>
    /// Gameplay-specific damage context with full Figure and Grid access.
    /// Implements IDamageContext for Core layer compatibility.
    /// </summary>
    public sealed class BeforeHitContext : IDamageContext
    {
        public Figure Attacker { get; set; }
        public Figure Target { get; set; }
        public BoardGrid Grid { get; set; }
        public float BaseDamage { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
        public float BonusDamage { get; set; }
        public bool IsCritical { get; set; }
        public bool IsDodged { get; set; }
        public bool IsCancelled { get; set; }
        public bool CanRetreat { get; set; }
        public bool TargetMovedThisTurn { get; set; }

        // IDamageContext explicit implementation for Core layer
        object IDamageContext.Attacker => Attacker;
        object IDamageContext.Target => Target;
    }
}
