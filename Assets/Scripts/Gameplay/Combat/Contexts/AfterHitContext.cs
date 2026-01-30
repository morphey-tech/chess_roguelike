using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    public sealed class AfterHitContext
    {
        public Figure Attacker { get; set; }
        public Figure Target { get; set; }
        public GridPosition AttackerPosition { get; set; }
        public GridPosition TargetPosition { get; set; }
        public BoardGrid Grid { get; set; }
        
        public int DamageDealt { get; set; }
        public bool TargetDied { get; set; }
        public int HealedAmount { get; set; }
        public bool WasCritical { get; set; }
    }
}
