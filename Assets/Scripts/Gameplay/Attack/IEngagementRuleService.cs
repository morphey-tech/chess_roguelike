using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    public interface IEngagementRuleService
    {
        bool IsEngaged(Figure unit, BoardGrid grid);
    }
}
