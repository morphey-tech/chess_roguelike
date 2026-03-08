using Project.Core.Core.Combat;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Figures
{
    public class FigureSpawnEntry
    {
        public string Id { get; set; }
        public Team Team { get; set; }
        public GridPosition Position { get; set; }
    }
}