using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.Gameplay.Combat.Threat
{
    /// <summary>
    /// Сервис построения карты угроз с кэшированием.
    /// Поддерживает инкрементальное обновление при перемещении фигур.
    /// </summary>
    public sealed class ThreatMapService
    {
        private readonly MovementService _movementService;
        private readonly IAttackQueryService _attackQueryService;
        private readonly ILogger<ThreatMapService> _logger;

        // Кэш карт угроз для каждой команды
        private readonly Dictionary<Team, ThreatMap> _threatMaps = new();

        // Кеш: фигура -> клетки, которые она атакует (для инкрементального обновления)
        private readonly Dictionary<Figure, List<GridPosition>> _figureThreats = new();

        // Флаг инкрементального режима
        private bool _isIncremental;

        public BoardGrid? Grid => _movementService.Grid;

        [Inject]
        private ThreatMapService(
            MovementService movementService,
            IAttackQueryService attackQueryService,
            ILogService logService)
        {
            _movementService = movementService;
            _attackQueryService = attackQueryService;
            _logger = logService.CreateLogger<ThreatMapService>();
        }

        /// <summary>
        /// Получить карту угроз для команды (из кэша или построить новую).
        /// </summary>
        public ThreatMap GetThreatMap(Team team)
        {
            if (_threatMaps.TryGetValue(team, out ThreatMap? map) && _isIncremental)
            {
                // Проверяем, не изменился ли размер сетки
                BoardGrid grid = _movementService.Grid;
                if (grid != null && (map.Width != grid.Width || map.Height != grid.Height))
                {
                    // Размер изменился, нужно перестроить
                    _logger.Debug($"GetThreatMap({team}): rebuild (size changed)");
                    BuildThreatMap(team);
                    return _threatMaps[team];
                }
                _logger.Debug($"GetThreatMap({team}): from cache");
                return map;
            }

            _logger.Debug($"GetThreatMap({team}): build new (cache miss, isIncremental={_isIncremental})");
            BuildThreatMap(team);

            return _threatMaps[team];
        }

        /// <summary>
        /// Построить карту угроз (полный пересчёт).
        /// </summary>
        private void BuildThreatMap(Team team)
        {
            BoardGrid grid = _movementService.Grid;
            if (grid == null)
                return;

            int width = grid.Width;
            int height = grid.Height;

            if (!_threatMaps.TryGetValue(team, out ThreatMap? map) ||
                map.Width != width ||
                map.Height != height)
            {
                map = new ThreatMap(width, height);
                _threatMaps[team] = map;
            }

            map.Clear();

            // Очищаем старые угрозы для фигур этой команды
            var figuresToRemove = new List<Figure>();
            foreach (var kvp in _figureThreats)
            {
                if (kvp.Key.Team == team)
                {
                    figuresToRemove.Add(kvp.Key);
                }
            }
            foreach (var figure in figuresToRemove)
            {
                _figureThreats.Remove(figure);
            }

            int threatCount = 0;
            foreach (Figure figure in grid.GetFiguresByTeam(team))
            {
                BoardCell? cell = grid.FindFigure(figure);
                if (cell == null)
                    continue;

                RegisterFigureThreat(figure, cell, map);
                threatCount++;
            }

            _isIncremental = true;
            _logger.Debug($"BuildThreatMap: team={team} figures={threatCount}");
        }

        /// <summary>
        /// Зарегистрировать угрозы от фигуры.
        /// </summary>
        private void RegisterFigureThreat(Figure figure, BoardCell cell, ThreatMap threatMap)
        {
            BoardGrid grid = _movementService.Grid;
            if (grid == null)
                return;

            int attackRange = figure.Stats.AttackRange;
            int minRow = Mathf.Max(0, cell.Position.Row - attackRange);
            int maxRow = Mathf.Min(grid.Height - 1, cell.Position.Row + attackRange);
            int minCol = Mathf.Max(0, cell.Position.Column - attackRange);
            int maxCol = Mathf.Min(grid.Width - 1, cell.Position.Column + attackRange);

            List<GridPosition> threatList = new();

            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    GridPosition pos = new(row, col);

                    // Пропускаем клетку самой фигуры
                    if (pos == cell.Position)
                        continue;

                    // Пропускаем клетку со своей фигурой
                    BoardCell? targetCell = grid.GetBoardCell(pos);
                    if (targetCell.OccupiedBy != null && targetCell.OccupiedBy.Team == figure.Team)
                        continue;

                    // Проверяем, может ли фигура атаковать эту клетку (включая пассивки)
                    if (_attackQueryService.CanAttackCell(figure, cell.Position, pos, grid))
                    {
                        threatMap.AddThreat(pos, figure);
                        threatList.Add(pos);
                    }
                }
            }

            _figureThreats[figure] = threatList;
            _logger.Debug($"RegisterFigureThreat: {figure.Id} team={figure.Team} pos=({cell.Position.Row},{cell.Position.Column}) threats={threatList.Count}");
        }

        /// <summary>
        /// Обновить угрозы от фигуры (после перемещения).
        /// </summary>
        public void UpdateFigureThreat(Figure figure)
        {
            BoardGrid grid = _movementService.Grid;
            if (grid == null)
            {
                _logger.Warning($"UpdateFigureThreat({figure.Id}): grid is null");
                return;
            }

            BoardCell? cell = grid.FindFigure(figure);
            if (cell == null)
            {
                // Фигура удалена с доски
                _logger.Debug($"UpdateFigureThreat({figure.Id}): figure not on grid, removing");
                RemoveFigureThreat(figure);
                return;
            }

            _logger.Debug($"UpdateFigureThreat({figure.Id}): team={figure.Team} pos=({cell.Position.Row},{cell.Position.Column}) isIncremental={_isIncremental}");

            // Если не в инкрементальном режиме, просто строим карту для команды
            if (!_isIncremental)
            {
                _logger.Debug($"UpdateFigureThreat({figure.Id}): building map for team {figure.Team}");
                BuildThreatMap(figure.Team);
                return;
            }

            // Удаляем старые угрозы
            RemoveFigureThreat(figure);

            // Получаем или создаём карту для команды фигуры
            if (!_threatMaps.TryGetValue(figure.Team, out ThreatMap? threatMap))
            {
                threatMap = new ThreatMap(grid.Width, grid.Height);
                _threatMaps[figure.Team] = threatMap;
                _logger.Debug($"UpdateFigureThreat({figure.Id}): created new threat map for team {figure.Team}");
            }

            // Добавляем новые угрозы
            RegisterFigureThreat(figure, cell, threatMap);
        }

        /// <summary>
        /// Удалить все угрозы от фигуры.
        /// </summary>
        public void RemoveFigureThreat(Figure figure)
        {
            if (!_figureThreats.TryGetValue(figure, out List<GridPosition>? cells))
                return;

            // Получаем карту для команды фигуры
            if (_threatMaps.TryGetValue(figure.Team, out ThreatMap? threatMap))
            {
                foreach (GridPosition pos in cells)
                {
                    threatMap.RemoveThreat(pos, figure);
                }
            }

            _figureThreats.Remove(figure);
        }

        /// <summary>
        /// Удалить все угрозы от фигуры по ID (когда фигура уже удалена из сетки).
        /// </summary>
        public void RemoveFigureThreatById(int figureId, Team team)
        {
            // Ищем фигуру по ID в кэше
            Figure? figureToRemove = null;
            foreach (var kvp in _figureThreats)
            {
                if (kvp.Key.Id == figureId)
                {
                    figureToRemove = kvp.Key;
                    break;
                }
            }

            if (figureToRemove == null)
                return;

            RemoveFigureThreat(figureToRemove);
        }

        /// <summary>
        /// Инвалидировать кэш (полный сброс).
        /// </summary>
        public void Invalidate()
        {
            _threatMaps.Clear();
            _figureThreats.Clear();
            _isIncremental = false;
        }
    }
}
