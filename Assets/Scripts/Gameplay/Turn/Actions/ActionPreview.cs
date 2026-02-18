using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Turn.Actions
{
    public sealed class ActionPreview
    {
        public GridPosition? MoveTo;
        public GridPosition? AttackPosition;
        public Figure? Target;
    }
}