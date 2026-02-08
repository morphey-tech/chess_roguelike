using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    public interface ITargetingService
    {
        bool CanTarget(GridPosition from,
                       GridPosition to,
                       AttackProfile attack,
                       BoardGrid grid,
                       Team attackerTeam);
    }
}
