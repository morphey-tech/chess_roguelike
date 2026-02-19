using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    public sealed class MoveContext
    {
        public Figure Actor { get; set; }
        public GridPosition From { get; set; }
        public GridPosition To { get; set; }
        public BoardGrid Grid { get; set; }
        public int CurrentTurn { get; set; }
        public bool DidMove { get; set; }
    }
}
