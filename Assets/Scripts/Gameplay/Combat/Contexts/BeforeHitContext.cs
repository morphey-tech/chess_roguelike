using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    public sealed class BeforeHitContext
    {
        public Figure Attacker { get; set; }
        public Figure Target { get; set; }
        public int BaseDamage { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
        public int BonusDamage { get; set; }
        public bool IsCritical { get; set; }
        public bool CanRetreat { get; set; }
    }
}
