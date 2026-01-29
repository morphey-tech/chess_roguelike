using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    public sealed class AfterHitContext
    {
        public Figure Attacker { get; set; }
        public Figure Target { get; set; }
        public int DamageDealt { get; set; }
        public bool TargetDied { get; set; }
        public int HealedAmount { get; set; }
        public bool WasCritical { get; set; }
    }
}
