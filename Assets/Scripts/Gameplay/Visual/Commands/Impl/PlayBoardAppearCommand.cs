using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command to play board appear animation by strategy id.
    /// </summary>
    public sealed class PlayBoardAppearCommand : IVisualCommand
    {
        private readonly string _strategyId;

        public string DebugName => $"PlayBoardAppear(strategy={_strategyId})";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public PlayBoardAppearCommand(string strategyId)
        {
            _strategyId = strategyId;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Board.PlayBoardAppearAsync(_strategyId);
        }
    }
}
