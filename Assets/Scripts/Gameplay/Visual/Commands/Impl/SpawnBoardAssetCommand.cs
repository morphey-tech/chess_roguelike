using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    public sealed class SpawnBoardAssetCommand : IVisualCommand
    {
        public string DebugName => $"Asset ={_boardAssetKey ?? "none"}, Appear={_appearStrategyId ?? "none"})";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;
        
        private readonly string? _boardAssetKey;
        private readonly string? _appearStrategyId;

        public SpawnBoardAssetCommand(string? boardAssetKey, string? appearStrategyId)
        {
            _boardAssetKey = boardAssetKey;
            _appearStrategyId = appearStrategyId;
        }
        
        public async UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            await presenters.Board.CreateBoardAssetAsync(_boardAssetKey, _appearStrategyId);
        }
    }
}