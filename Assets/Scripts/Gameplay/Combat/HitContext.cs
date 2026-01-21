using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat
{
    public class HitContext
    {
        public Figure Attacker { get; set; }
        public Figure Target { get; set; }
        public GridPosition AttackerPosition { get; set; }
        public GridPosition TargetPosition { get; set; }
        public BoardGrid Grid { get; set; }
        public int BaseDamage { get; set; }
        public int FinalDamage { get; set; }
        public HitType HitType { get; set; }
        public bool AttackerMovesOnKill { get; set; }
        public int HitCount { get; set; } = 1;
        public float DamageMultiplier { get; set; } = 1f;
        public int HealedAmount { get; set; }
        public bool IsCritical { get; set; }
        public bool TargetDied { get; set; }
    }
}
