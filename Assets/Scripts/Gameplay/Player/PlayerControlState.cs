using System;
using Project.Core.Core.Blockers;
using Project.Core.Core.State;
using Project.Core.Player;
using UniRx;

namespace Project.Gameplay.Gameplay.Player
{
    public sealed class PlayerControlState : IPlayerControlState, IDisposable
    {
        IReadOnlyReactiveProperty<bool> IPlayerControlState.CanMove => _canMove;
        IReadOnlyReactiveProperty<bool> IPlayerControlState.CanLook => _canLook;

        private readonly ReactiveProperty<bool> _canMove = new(true);
        private readonly ReactiveProperty<bool> _canLook = new(true);
        private readonly CompositeDisposable _disposables = new();

        public PlayerControlState(
            IGameStateService gameState,
            IPlayerBlockerService blockers)
        {
            gameState.State
                .CombineLatest(blockers.Blockers,
                    (state, block) =>
                        state == GameState.Gameplay &&
                        !block.HasFlag(PlayerBlocker.MovementBlock))
                .DistinctUntilChanged()
                .Subscribe(v => _canMove.Value = v)
                .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
            _canMove.Dispose();
        }
    }
}