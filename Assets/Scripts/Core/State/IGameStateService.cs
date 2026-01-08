using UniRx;

namespace Project.Core.Core.State
{
    public interface IGameStateService
    {
        IReadOnlyReactiveProperty<GameState> State { get; }
        void Set(GameState state);
    }
}