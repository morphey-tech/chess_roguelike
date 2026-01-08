using System;
using UniRx;

namespace Project.Core.Core.State
{
    public sealed class GameStateService : IGameStateService, IDisposable
    {
        IReadOnlyReactiveProperty<GameState> IGameStateService.State => _state;

        private readonly ReactiveProperty<GameState> _state = new(GameState.Gameplay);
        
        void IGameStateService.Set(GameState state)
        {
            if (_state.Value == state)
            {
                return;
            }
            _state.Value = state;
        }

        void IDisposable.Dispose()
        {
            _state.Dispose();
        }
    }
}