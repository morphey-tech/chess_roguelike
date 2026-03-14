using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
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

        private ThreatMap? _cachedMap;
        private Team? _cachedTeam;

        public ThreatMapService(
            MovementService movementService,
            IAttackQueryService attackQueryService)
        {
            _movementService = movementService;
            _attackQueryService = attackQueryService;
        }

        /// <summary>
        /// Получить карту угроз для команды (из кэша или построить новую).
        /// </summary>
        public ThreatMap GetThreatMap(Team team)
        {
            if (_cachedMap != null && _cachedTeam == team)
                return _cachedMap;

            BuildThreatMap(team);

            return _cachedMap!;
        }

        /// <summary>
        /// Построить карту угроз.
        /// </summary>
        private void BuildThreatMap(Team team)
        {
            BoardGrid grid = _movementService.Grid;
            
            if (grid == null)
                return;

            int width = grid.Width;
            int height = grid.Height;

            if (_cachedMap == null || _cachedMap.Width != width || _cachedMap.Height != height)
                _cachedMap = new ThreatMap(width, height);

            _cachedMap.Clear();

            foreach (Figure figure in grid.GetFiguresByTeam(team))
            {
                BoardCell? cell = grid.FindFigure(figure);
                if (cell == null)
                    continue;

                // Оптимизация: проверяем только клетки в диапазоне атаки
                int attackRange = figure.Stats.AttackRange;
                int minRow = Mathf.Max(0, cell.Position.Row - attackRange);
                int maxRow = Mathf.Min(height - 1, cell.Position.Row + attackRange);
                int minCol = Mathf.Max(0, cell.Position.Column - attackRange);
                int maxCol = Mathf.Min(width - 1, cell.Position.Column + attackRange);

                for (int row = minRow; row <= maxRow; row++)
                {
                    for (int col = minCol; col <= maxCol; col++)
                    {
                        GridPosition pos = new(row, col);
                        
                        // Пропускаем клетку самой фигуры
                        if (pos == cell.Position)
                            continue;
                        
                        if (_attackQueryService.CanAttackCell(figure, cell.Position, pos, grid))
                        {
                            _cachedMap.AddThreat(pos, figure);
                        }
                    }
                }
            }

            _cachedTeam = team;
        }

        /// <summary>
        /// Инвалидировать кэш (например, после хода фигуры).
        /// </summary>
        public void Invalidate()
        {
            _cachedTeam = null;
        }
    }
}
