using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using VContainer;

namespace Project.Gameplay.Gameplay.Combat.Threat
{
    public sealed class ThreatMapService
    {
        private readonly MovementService _movementService;
        private readonly IAttackQueryService _attackQueryService;
        private readonly ILogger<ThreatMapService> _logger;

        private readonly Dictionary<Team, ThreatMap> _threatMaps = new Dictionary<Team, ThreatMap>();
        private readonly Dictionary<Team, Dictionary<Figure, List<GridPosition>>> _figureThreats = new Dictionary<Team, Dictionary<Figure, List<GridPosition>>>();
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

        public ThreatMap GetThreatMap(Team team)
        {
            if (_threatMaps.TryGetValue(team, out ThreatMap? map) && _isIncremental)
            {
                BoardGrid grid = _movementService.Grid;
                if (grid != null && (map.Width != grid.Width || map.Height != grid.Height))
                {
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

            if (!_figureThreats.TryGetValue(team, out Dictionary<Figure, List<GridPosition>>? teamFigureThreats))
            {
                teamFigureThreats = new Dictionary<Figure, List<GridPosition>>();
                _figureThreats[team] = teamFigureThreats;
            }
            teamFigureThreats.Clear();

            int threatCount = 0;
            foreach (Figure figure in grid.GetFiguresByTeam(team))
            {
                BoardCell? cell = grid.FindFigure(figure);
                if (cell == null)
                    continue;

                RegisterFigureThreat(figure, cell, map, teamFigureThreats);
                threatCount++;
            }

            _isIncremental = true;
            _logger.Debug($"BuildThreatMap: team={team} figures={threatCount}");
        }

        private void RegisterFigureThreat(Figure figure, BoardCell cell, ThreatMap threatMap, Dictionary<Figure, List<GridPosition>> teamFigureThreats)
        {
            IAttackStrategy strategy = _attackQueryService.GetStrategy(figure.AttackId);
            List<GridPosition> threatList = new List<GridPosition>();

            foreach (GridPosition pos in strategy.GetAttackPositions(figure, cell.Position, _movementService.Grid))
            {
                if (_attackQueryService.CanAttackCell(figure, cell.Position, pos, _movementService.Grid))
                {
                    threatMap.AddThreat(pos, figure);
                    threatList.Add(pos);
                }
            }

            teamFigureThreats[figure] = threatList;
            _logger.Debug($"RegisterFigureThreat: {figure.Id} team={figure.Team} pos=({cell.Position.Row},{cell.Position.Column}) threats={threatList.Count}");
        }

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
                _logger.Debug($"UpdateFigureThreat({figure.Id}): figure not on grid, removing");
                RemoveFigureThreat(figure);
                return;
            }

            _logger.Debug($"UpdateFigureThreat({figure.Id}): team={figure.Team} pos=({cell.Position.Row},{cell.Position.Column}) isIncremental={_isIncremental}");

            if (!_isIncremental)
            {
                _logger.Debug($"UpdateFigureThreat({figure.Id}): building map for team {figure.Team}");
                BuildThreatMap(figure.Team);
                return;
            }

            RemoveFigureThreat(figure);

            if (!_threatMaps.TryGetValue(figure.Team, out ThreatMap? threatMap))
            {
                threatMap = new ThreatMap(grid.Width, grid.Height);
                _threatMaps[figure.Team] = threatMap;
                _logger.Debug($"UpdateFigureThreat({figure.Id}): created new threat map for team {figure.Team}");
            }

            if (!_figureThreats.TryGetValue(figure.Team, out Dictionary<Figure, List<GridPosition>>? teamFigureThreats))
            {
                teamFigureThreats = new Dictionary<Figure, List<GridPosition>>();
                _figureThreats[figure.Team] = teamFigureThreats;
            }

            RegisterFigureThreat(figure, cell, threatMap, teamFigureThreats);
        }

        public void RemoveFigureThreat(Figure figure)
        {
            if (!_figureThreats.TryGetValue(figure.Team, out Dictionary<Figure, List<GridPosition>>? teamFigureThreats))
                return;

            if (!teamFigureThreats.TryGetValue(figure, out List<GridPosition>? cells))
                return;

            if (_threatMaps.TryGetValue(figure.Team, out ThreatMap? threatMap))
            {
                foreach (GridPosition pos in cells)
                {
                    threatMap.RemoveThreat(pos, figure);
                }
            }

            teamFigureThreats.Remove(figure);
        }

        public void RemoveFigureThreatById(int figureId, Team team)
        {
            if (!_figureThreats.TryGetValue(team, out Dictionary<Figure, List<GridPosition>>? teamFigureThreats))
                return;

            Figure? figureToRemove = null;
            foreach (KeyValuePair<Figure, List<GridPosition>> kvp in teamFigureThreats)
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

        public void Invalidate()
        {
            _threatMaps.Clear();
            _figureThreats.Clear();
            _isIncremental = false;
        }
    }
}
