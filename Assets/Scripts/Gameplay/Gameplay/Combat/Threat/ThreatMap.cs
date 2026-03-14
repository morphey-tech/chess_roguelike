using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Threat
{
    /// <summary>
    /// Карта угроз на основе массива.
    /// O(1) доступ к любой клетке без хеширования.
    /// </summary>
    public sealed class ThreatMap
    {
        private readonly List<Figure>[,] _grid;

        public int Width { get; }
        public int Height { get; }

        public ThreatMap(int width, int height)
        {
            Width = width;
            Height = height;

            // [row, column] = [height, width]
            _grid = new List<Figure>[height, width];
        }

        /// <summary>
        /// Очистить все угрозы.
        /// </summary>
        public void Clear()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    _grid[row, col]?.Clear();
                }
            }
        }

        /// <summary>
        /// Добавить угрозу для клетки.
        /// </summary>
        public void AddThreat(GridPosition pos, Figure attacker)
        {
            if (!IsValidPosition(pos))
                return;

            var list = _grid[pos.Row, pos.Column];

            if (list == null)
            {
                list = new List<Figure>(2);
                _grid[pos.Row, pos.Column] = list;
            }

            list.Add(attacker);
        }

        /// <summary>
        /// Удалить угрозу от конкретной фигуры для клетки.
        /// </summary>
        public void RemoveThreat(GridPosition pos, Figure attacker)
        {
            if (!IsValidPosition(pos))
                return;

            var list = _grid[pos.Row, pos.Column];
            if (list == null)
                return;

            list.Remove(attacker);

            if (list.Count == 0)
                _grid[pos.Row, pos.Column] = null;
        }

        /// <summary>
        /// Проверить, находится ли клетка под угрозой.
        /// </summary>
        public bool IsThreatened(GridPosition pos)
        {
            if (!IsValidPosition(pos))
                return false;

            var list = _grid[pos.Row, pos.Column];
            return list != null && list.Count > 0;
        }

        /// <summary>
        /// Получить всех атакующих фигуру на клетке.
        /// </summary>
        public IReadOnlyList<Figure>? GetAttackers(GridPosition pos)
        {
            if (!IsValidPosition(pos))
                return null;

            return _grid[pos.Row, pos.Column];
        }

        private bool IsValidPosition(GridPosition pos)
        {
            return pos.Row >= 0 && pos.Row < Height && 
                   pos.Column >= 0 && pos.Column < Width;
        }
    }
}
