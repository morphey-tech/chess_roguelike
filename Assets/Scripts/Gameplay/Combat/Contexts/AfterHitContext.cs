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
        
        /// <summary>
        /// If a passive moves the attacker, set this to the new position.
        /// </summary>
        public GridPosition? AttackerMovedTo { get; set; }
        
        /// <summary>
        /// If set, the attacker should get a bonus move with this max distance.
        /// </summary>
        public int? BonusMoveDistance { get; set; }
    }
}
