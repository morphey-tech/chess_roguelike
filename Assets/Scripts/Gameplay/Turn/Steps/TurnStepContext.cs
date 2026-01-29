using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Turn.Steps
{
    public sealed class TurnStepContext
    {
        public Figure Actor { get; set; }
        public BoardGrid Grid { get; set; }
        public GridPosition From { get; set; }
        public GridPosition To { get; set; }
        
        public bool LastAttackKilledTarget { get; set; }
        public int LastDamageDealt { get; set; }
    }
}
