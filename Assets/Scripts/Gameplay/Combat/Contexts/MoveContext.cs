using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    public sealed class MoveContext
    {
        public Figure Figure { get; set; }
        public GridPosition From { get; set; }
        public GridPosition To { get; set; }
        public bool DidMove { get; set; }
        public int DamageReduction { get; set; }
    }
}
