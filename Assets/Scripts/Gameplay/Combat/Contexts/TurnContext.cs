using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    public sealed class TurnContext
    {
        public BoardGrid Grid { get; set; }
        public Team Team { get; set; }
        public int CurrentTurn { get; set; }
    }
}
