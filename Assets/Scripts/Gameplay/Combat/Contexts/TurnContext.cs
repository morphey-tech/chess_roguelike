using Project.Core.Core.Combat;
using Project.Core.Core.Combat.Contexts;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    /// <summary>
    /// Gameplay-specific turn context with full Grid access.
    /// Implements ITurnContext for Core layer compatibility.
    /// </summary>
    public sealed class TurnContext : ITurnContext
    {
        public BoardGrid Grid { get; set; }
        public Team Team { get; set; }
        public int CurrentTurn { get; set; }

        // ITurnContext explicit implementation for Core layer
        int ITurnContext.TurnNumber => CurrentTurn;
    }
}
