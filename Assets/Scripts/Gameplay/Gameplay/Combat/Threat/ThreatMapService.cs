using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Combat.Threat
{
    /// <summary>
    /// Сервис построения карты угроз с кэшированием.
    /// Кэш инвалидируется при смене команды или явном вызове Invalidate().
    /// </summary>
    public sealed class ThreatMapService
    {
        private readonly MovementService _movementService;
        private readonly IAttackQueryService _attackQueryService;
        private readonly ILogger<ThreatMapService> _logger;

        private ThreatMap? _cachedMap;
        private Team? _cachedTeam;

        public ThreatMapService(
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
            if (_cachedMap != null && _cachedTeam == team)
            {
                _logger.Debug($"ThreatMap cache hit for team {team}");
                return _cachedMap;
            }

            _logger.Debug($"ThreatMap cache miss for team {team}, building...");
            BuildThreatMap(team);
            _logger.Debug($"ThreatMap built, cached team = {_cachedTeam}");

            return _cachedMap!;
        }

        /// <summary>
        /// Построить карту угроз.
        /// </summary>
        private void BuildThreatMap(Team team)
        {
            BoardGrid grid = _movementService.Grid;
            
            if (grid == null)
            {
                _logger.Warning("ThreatMapService: Grid is null!");
                return;
            }

            int width = grid.Width;
            int height = grid.Height;

            if (_cachedMap == null || _cachedMap.Width != width || _cachedMap.Height != height)
            {
                _logger.Debug($"Creating new ThreatMap {width}x{height}");
                _cachedMap = new ThreatMap(width, height);
            }

            _cachedMap.Clear();

            int figureCount = 0;
            int threatCount = 0;
            
            foreach (Figure figure in grid.GetFiguresByTeam(team))
            {
                figureCount++;
                BoardCell? cell = grid.FindFigure(figure);
                if (cell == null)
                    continue;

                // Проверяем все клетки сетки на возможность атаки
                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        GridPosition pos = new(row, col);
                        
                        // Пропускаем клетку самой фигуры
                        if (pos == cell.Position)
                            continue;
                        
                        if (_attackQueryService.CanAttackCell(figure, cell.Position, pos, grid))
                        {
                            _cachedMap.AddThreat(pos, figure);
                            threatCount++;
                        }
                    }
                }
            }
            
            _logger.Debug($"ThreatMap built: {figureCount} figures, {threatCount} threats for team {team}");
            _cachedTeam = team;
        }

        /// <summary>
        /// Инвалидировать кэш (например, после хода фигуры).
        /// </summary>
        public void Invalidate()
        {
            _logger.Debug($"ThreatMap invalidated");
            _cachedTeam = null;
        }
    }
}
