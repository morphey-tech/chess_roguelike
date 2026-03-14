using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    /// <summary>
    /// Контекст для фильтрации целей атаки.
    /// </summary>
    public sealed class AttackContext
    {
        public Figure Attacker { get; }
        public GridPosition From { get; }
        public BoardGrid Grid { get; }

        public AttackContext(Figure attacker, GridPosition from, BoardGrid grid)
        {
            Attacker = attacker;
            From = from;
            Grid = grid;
        }
    }

    /// <summary>
    /// Фильтр для модификации списка целей атаки.
    /// Используется пассивными способностями для добавления, удаления или изменения целей.
    /// </summary>
    public interface IAttackFilter
    {
        /// <summary>
        /// Модифицирует список целей атаки.
        /// Может добавлять, удалять или изменять цели.
        /// </summary>
        void FilterTargets(List<GridPosition> targets, AttackContext context);
    }

    /// <summary>
    /// Модификатор диапазона атаки.
    /// Используется пассивными способностями для изменения диапазона атаки.
    /// </summary>
    public interface IAttackRangeModifier
    {
        /// <summary>
        /// Проверяет, может ли фигура атаковать клетку.
        /// Возвращает true если пассивка разрешает атаку на эту клетку.
        /// </summary>
        bool CanAttackCell(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid);
    }
}
