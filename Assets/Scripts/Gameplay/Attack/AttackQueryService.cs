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
    /// <summary>
    /// Сервис для проверки возможностей атаки.
    /// </summary>
    public sealed class AttackQueryService : IAttackQueryService
    {
        private readonly AttackStrategyFactory _strategyFactory;

        [Inject]
        private AttackQueryService(AttackStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        /// <summary>
        /// Возвращает все клетки, куда может атаковать фигура с текущей позиции.
        /// </summary>
        public IReadOnlyCollection<GridPosition> GetTargets(Figure? actor, GridPosition from, BoardGrid? grid)
        {
            if (actor == null || grid == null)
            {
                return new List<GridPosition>();
            }
            
            var strategy = _strategyFactory.Get(actor.AttackId);
            var targets = new List<GridPosition>();
            
            foreach (var enemy in grid.GetFiguresByTeam(actor.Team == Team.Player 
                         ? Team.Enemy : Team.Player))
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
        /// Проверяет, может ли атакующий атаковать конкретную клетку.
        /// Проверяет направление и дистанцию атаки (без проверки наличия цели).
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
            if (HasActiveDesperation(attacker, grid))
            {
                return AttackUtils.GetDistance(from, targetCell) == 1;
            }
            
            // Получаем стратегию атаки и проверяем направление
            var strategy = _strategyFactory.Get(attacker.AttackId);
            return CanAttackPosition(strategy, attacker, from, targetCell, grid);
        }
        
        /// <summary>
        /// Проверяет, может ли стратегия атаки атаковать позицию (без проверки наличия фигуры).
        /// </summary>
        private static bool CanAttackPosition(IAttackStrategy strategy, Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            // Проверяем дистанцию
            if (!AttackUtils.IsInRange(from, to, attacker.Stats.AttackRange))
            {
                return false;
            }
            
            // Проверяем направление в зависимости от стратегии
            return strategy switch
            {
                SimpleAttack => true,
                RangedAttack => true,
                PawnAttack => CanAttackPosition((PawnAttack)strategy, attacker, from, to),
                DiagonalAttack => CanAttackPosition((DiagonalAttack)strategy, from, to),
                _ => true
            };
        }
        
        /// <summary>
        /// Проверяет, может ли PawnAttack атаковать позицию.
        /// Пешка атакует только по диагонали вперед на 1 клетку.
        /// </summary>
        private static bool CanAttackPosition(PawnAttack strategy, Figure attacker, GridPosition from, GridPosition to)
        {
            int rowDiff = to.Row - from.Row;
            int colDiff = Mathf.Abs(to.Column - from.Column);
            
            // Пешка атакует только по диагонали (|rowDiff| == 1, |colDiff| == 1)
            if (Mathf.Abs(rowDiff) != 1 || colDiff != 1)
            {
                return false;
            }
            
            // Проверяем направление (вперед)
            bool isPlayer = attacker.Team == Team.Player;
            return (isPlayer && rowDiff > 0) || (!isPlayer && rowDiff < 0);
        }
        
        /// <summary>
        /// Проверяет, может ли DiagonalAttack атаковать позицию.
        /// Атакует только по диагонали (|rowDiff| == |colDiff|).
        /// </summary>
        private static bool CanAttackPosition(DiagonalAttack strategy, GridPosition from, GridPosition to)
        {
            int rowDiff = Mathf.Abs(to.Row - from.Row);
            int colDiff = Mathf.Abs(to.Column - from.Column);
            
            // Диагональ: |rowDiff| == |colDiff| и не 0
            return rowDiff == colDiff && rowDiff > 0;
        }
        
        /// <summary>
        /// Проверяет, активен ли у фигуры Desperation (нет союзников рядом).
        /// </summary>
        private static bool HasActiveDesperation(Figure figure, BoardGrid grid)
        {
            if (!figure.BasePassives.Any(p => p is DesperationPassive))
            {
                return false;
            }
            
            return grid.CountAlliesAround(figure) == 0;
        }
    }
}
