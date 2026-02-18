using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Turn
{
    /// <summary>
    /// Unified context for turn actions - used by both Conditions and Steps.
    /// </summary>
    public sealed class ActionContext
    {
        public Figure Actor { get; set; }
        public BoardGrid Grid { get; set; }
        public GridPosition To { get; set; }
        public IReadOnlyList<Figure> Enemies { get; set; }
        public MovementService MovementService { get; set; }
        
        /// <summary>
        /// Current position of the actor. Updated when actor moves.
        /// </summary>
        public GridPosition From { get; set; }
        
        public bool LastAttackKilledTarget { get; set; }
        public float LastDamageDealt { get; set; }
        

        /// <summary>
        /// Set by steps when an action actually happens (move/attack).
        /// Used to avoid consuming a turn on invalid actions.
        /// </summary>
        public bool ActionExecuted { get; set; }
        
        /// <summary>
        /// If set, the actor gets a bonus move after the current step.
        /// </summary>
        public int? BonusMoveDistance { get; set; }
        
        /// <summary>
        /// Alias for From (actor's current position).
        /// </summary>
        public GridPosition ActorPosition => From;
        
        /// <summary>
        /// Target position (nullable for conditions that check without target).
        /// </summary>
        public GridPosition? TargetPosition => To;
    }
}
