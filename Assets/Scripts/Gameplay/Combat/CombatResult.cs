using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat
{
    public struct CombatResult
    {
        public int DamageDealt { get; set; }
        public bool TargetDied { get; set; }
        public int HealedAmount { get; set; }
        public bool AttackerMoves { get; set; }
        public bool WasCritical { get; set; }
        
        /// <summary>
        /// Results for additional targets (splash, pierce, etc.)
        /// </summary>
        public List<AdditionalTargetResult> AdditionalResults { get; set; }
        
        /// <summary>
        /// If a passive moved the attacker (e.g., retreat), this is the new position.
        /// </summary>
        public GridPosition? AttackerMovedTo { get; set; }
        
        /// <summary>
        /// If set, the attacker should get a bonus move with this max distance.
        /// Used for mechanics like "slippery" where player chooses where to retreat.
        /// </summary>
        public int? BonusMoveDistance { get; set; }
        
        /// <summary>
        /// If a passive pushed the target (e.g., brutal), this is the new position.
        /// </summary>
        public GridPosition? TargetPushedTo { get; set; }
        
        /// <summary>
        /// Additional damage dealt by passives (e.g., push blocked damage).
        /// </summary>
        public int BonusDamageDealt { get; set; }
    }

    public struct AdditionalTargetResult
    {
        public Figure Target { get; set; }
        public int DamageDealt { get; set; }
        public bool Died { get; set; }
    }
}
