using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    public sealed class BeforeHitContext
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
    }
}
