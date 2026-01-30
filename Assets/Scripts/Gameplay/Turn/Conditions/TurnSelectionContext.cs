using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Turn.Conditions
{
    public sealed class TurnSelectionContext
    {
        public Figure Actor { get; set; }
        public BoardGrid Grid { get; set; }
        public GridPosition ActorPosition { get; set; }
        public GridPosition? TargetPosition { get; set; }
        public List<Figure> Enemies { get; set; }
        public MovementService MovementService { get; set; }
    }
}
