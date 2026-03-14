using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack.Strategies;
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
        private readonly ILogger<AttackQueryService> _logger;

        [Inject]
        private AttackQueryService(AttackStrategyFactory strategyFactory, ILogService logService)
        {
            _strategyFactory = strategyFactory;
            _logger = logService.CreateLogger<AttackQueryService>();
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

            var targets = new List<GridPosition>();

            // Базовые цели по стратегии атаки
            var strategy = _strategyFactory.Get(actor.AttackId);
            foreach (var enemy in grid.GetFiguresByTeam(actor.Team == Team.Player
                         ? Team.Enemy : Team.Player))
            {
                var cell = grid.FindFigure(enemy);
                if (cell != null && strategy.CanAttack(actor, from, cell.Position, grid))
                {
                    targets.Add(cell.Position);
                }
            }

            int baseCount = targets.Count;

            // Применяем фильтры от пассивок
            var context = new AttackContext(actor, from, grid);
            foreach (var passive in actor.BasePassives.OfType<IAttackFilter>())
            {
                _logger.Debug($"GetTargets: applying filter {passive.GetType().Name} for {actor.Id}");
                passive.FilterTargets(targets, context);
            }

            _logger.Debug($"GetTargets: {actor.Id} from=({from.Row},{from.Column}) base={baseCount} filters={targets.Count - baseCount} total={targets.Count}");
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

            // Нельзя атаковать клетку со своей фигурой
            BoardCell? cell = grid.GetBoardCell(targetCell);
            if (cell.OccupiedBy != null && cell.OccupiedBy.Team == attacker.Team)
            {
                return false;
            }

            // Проверяем модификаторы от пассивок (например Desperation)
            foreach (var passive in attacker.BasePassives.OfType<IAttackRangeModifier>())
            {
                if (passive.CanAttackCell(attacker, from, targetCell, grid))
                {
                    _logger.Debug($"CanAttackCell: {attacker.Id} -> ({targetCell.Row},{targetCell.Column}) = true (via {passive.GetType().Name})");
                    return true;
                }
            }

            // Получаем стратегию атаки и проверяем направление
            var strategy = _strategyFactory.Get(attacker.AttackId);
            bool result = CanAttackPosition(strategy, attacker, from, targetCell, grid);
            _logger.Debug($"CanAttackCell: {attacker.Id} -> ({targetCell.Row},{targetCell.Column}) = {result} (via {strategy.Id})");
            return result;
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
    }
}
