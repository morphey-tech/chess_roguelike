using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Turn.Execution
{
    /// <summary>
    /// Result of executing a turn step.
    /// </summary>
    public struct TurnExecutionResult
    {
        /// <summary>
        /// Whether the turn was executed successfully.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// The final position of the actor after the turn.
        /// </summary>
        public GridPosition ActorFinalPosition { get; set; }
        
        /// <summary>
        /// If set, the actor should get a bonus move with this max distance.
        /// </summary>
        public int? BonusMoveDistance { get; set; }
        
        /// <summary>
        /// Whether the last attack killed the target.
        /// </summary>
        public bool KilledTarget { get; set; }
        
        public static TurnExecutionResult Failed => new() { Success = false };
    }
}
