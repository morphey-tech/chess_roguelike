using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    public interface IAttackResolver
    {
        AttackProfile Resolve(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid);
    }
}
