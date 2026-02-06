using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Board
{
    /// <summary>
    /// Port for board visualization. Gameplay knows nothing about Unity.
    /// </summary>
    public interface IBoardPresenter
    {
        UniTask CreateCell(Entity entity, GridPosition pos, string skinId);
        /// <summary>
        /// Создаёт все клетки (параллельно), затем при необходимости запускает одну анимацию появления.
        /// Визуально: все клетки появляются вместе, без «по одной».
        /// </summary>
        UniTask CreateCellsBatchAsync(IReadOnlyList<CellSpawnRequest> requests, string? appearStrategyId);
        UniTask DestroyCell(GridPosition pos);
        UniTask PlayBoardAppearAsync(string strategyId);
        void Clear();
    }
}
