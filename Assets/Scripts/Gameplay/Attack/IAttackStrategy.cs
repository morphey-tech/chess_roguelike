using System.Collections.Generic;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    public interface IAttackStrategy
    {
        string Id { get; }
        DeliveryType Delivery { get; }
        bool CanAttack(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid);
        HitContext CreateHitContext(Figure attacker, Figure defender, GridPosition attackerPos, GridPosition defenderPos, BoardGrid grid);
        bool CanAttackPosition(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid);
        IEnumerable<GridPosition> GetAttackPositions(Figure attacker, GridPosition from, BoardGrid grid);
    }
}
