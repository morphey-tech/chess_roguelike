using Project.Core.Core.Combat.Contexts;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    /// <summary>
    /// Gameplay-specific move context with full Figure and Grid access.
    /// Implements IMoveContext for Core layer compatibility.
    /// </summary>
    public sealed class MoveContext : IMoveContext
    {
        public Figure Actor { get; set; }
        public GridPosition From { get; set; }
        public GridPosition To { get; set; }
        public BoardGrid Grid { get; set; }
        public int CurrentTurn { get; set; }
        public bool DidMove { get; set; }

        // IMoveContext explicit implementation for Core layer
        object IMoveContext.Actor => Actor;
    }
}
