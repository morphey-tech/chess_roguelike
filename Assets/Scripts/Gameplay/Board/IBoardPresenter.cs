using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Board
{
    public interface IBoardPresenter
    {
        UniTask CreateBoardAssetAsync(string? assetKey, string? appearStrategyId);   
        UniTask CreateCellsBatchAsync(IReadOnlyList<CellSpawnRequest> requests, string? appearStrategyId);
        
        UniTask DestroyCell(GridPosition pos);
        void Clear();
    }
}
