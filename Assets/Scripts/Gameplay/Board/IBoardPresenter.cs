using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Board
{
    /// <summary>
    /// Port for board visualization. Gameplay knows nothing about Unity.
    /// </summary>
    public interface IBoardPresenter
    {
        void CreateCell(GridPosition pos, string skinId);
        void DestroyCell(GridPosition pos);

        void PlayAppear(GridPosition pos);
        void PlayHit(GridPosition pos);
        void Highlight(GridPosition pos, bool enabled);
        
        // Board-wide operations
        UniTask PlayBoardAppearAsync(string strategyId);
        void Clear();
    }
}
