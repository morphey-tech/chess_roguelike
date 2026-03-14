using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Attack.Strategies;
using Project.Gameplay.Gameplay.Combat.Passives;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.Gameplay.Attack
{
    public sealed class AttackQueryService : IAttackQueryService
    {
        private readonly AttackStrategyFactory _strategyFactory;

        [Inject]
        private AttackQueryService(AttackStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        public IReadOnlyCollection<GridPosition> GetTargets(Figure? actor, GridPosition from, BoardGrid? grid)
        {
            if (actor == null || grid == null)
            {
                return new List<GridPosition>();
            }
            
            var strategy = _strategyFactory.Get(actor.AttackId);
            var targets = new List<GridPosition>();
            
            foreach (var enemy in grid.GetFiguresByTeam(actor.Team == Team.Player ? Team.Enemy : Team.Player))
            {
                var cell = grid.FindFigure(enemy);
                if (cell != null && strategy.CanAttack(actor, from, cell.Position, grid))
                {
                    targets.Add(cell.Position);
                }
            }
            
            return targets;
        }

        /// <summary>
        /// Проверяет, может ли атакующий атаковать конкретную клетку (независимо от того, есть ли там фигура).
        /// Проверяет только направление и дистанцию атаки.
        /// </summary>
        public bool CanAttackCell(Figure attacker, GridPosition from, GridPosition targetCell, BoardGrid grid)
        {
            // Нельзя атаковать свою собственную клетку
            if (from == targetCell)
            {
                return false;
            }
            
            if (!grid.IsInside(targetCell))
            {
                return false;
            }
            
            // Проверяем Desperation - если активен и нет союзников рядом, может атаковать любую соседнюю клетку
            if (attacker.BasePassives.Any(p => p is DesperationPassive))
            {
                int allies = grid.CountAlliesAround(attacker);
                if (allies == 0)
                {
                    int distance = AttackUtils.GetDistance(from, targetCell);
                    return distance == 1;
                }
            }
            
            // Получаем стратегию атаки и проверяем направление
            var strategy = _strategyFactory.Get(attacker.AttackId);
            
            // Проверяем дистанцию
            if (!AttackUtils.IsInRange(from, targetCell, attacker.Stats.AttackRange))
            {
                return false;
            }
            
            // Для разных стратегий - разная логика проверки направления
            return strategy switch
            {
                SimpleAttack => true, // Может атаковать в любую сторону в пределах range
                RangedAttack => true, // Может атаковать в любую сторону в пределах range
                PawnAttack pawn => CanAttackPosition(pawn, attacker, from, targetCell, grid),
                DiagonalAttack diagonal => CanAttackPosition(diagonal, attacker, from, targetCell, grid),
                _ => true
            };
        }
        
        /// <summary>
        /// Проверяет, может ли PawnAttack атаковать позицию (без проверки наличия фигуры).
        /// Пешка атакует только по диагонали вперед.
        /// </summary>
        private bool CanAttackPosition(PawnAttack strategy, Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            int rowDiff = to.Row - from.Row;
            int colDiff = Mathf.Abs(to.Column - from.Column);
            
            // Пешка атакует только по диагонали (rowDiff != 0, colDiff == 1)
            if (colDiff != 1 || Mathf.Abs(rowDiff) != 1)
            {
                return false;
            }
            
            // Проверяем направление (вперед)
            bool isPlayer = attacker.Team == Team.Player;
            return (isPlayer && rowDiff > 0) || (!isPlayer && rowDiff < 0);
        }
        
        /// <summary>
        /// Проверяет, может ли DiagonalAttack атаковать позицию (без проверки наличия фигуры).
        /// Атакует только по диагонали.
        /// </summary>
        private bool CanAttackPosition(DiagonalAttack strategy, Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            int rowDiff = Mathf.Abs(to.Row - from.Row);
            int colDiff = Mathf.Abs(to.Column - from.Column);
            
            // Диагональ: rowDiff == colDiff
            return rowDiff == colDiff && rowDiff > 0;
        }
    }
}
