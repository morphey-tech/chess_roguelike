using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Одна команда: создать все клетки доски (батчем) и при необходимости проиграть анимацию появления.
    /// Клетки создаются параллельно, затем одна общая анимация — без «появления по одной».
    /// </summary>
    public sealed class SpawnBoardCellsCommand : IVisualCommand
    {
        private readonly IReadOnlyList<CellSpawnRequest> _requests;
        private readonly string? _appearStrategyId;

        public string DebugName => $"SpawnBoardCells(count={_requests.Count}, appear={_appearStrategyId ?? "none"})";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public SpawnBoardCellsCommand(IReadOnlyList<CellSpawnRequest> requests, string? appearStrategyId)
        {
            _requests = requests;
            _appearStrategyId = appearStrategyId;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Board.CreateCellsBatchAsync(_requests, _appearStrategyId);
        }
    }
}
