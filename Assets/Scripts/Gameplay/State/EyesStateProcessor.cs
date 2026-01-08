using System;
using MessagePipe;
using Project.Core.Core.Blockers;
using Project.Core.Core.State;
using Project.Gameplay.Gameplay.Player;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.State
{
    public sealed class EyesStateProcessor : IStartable, IDisposable
    {
        private readonly IGameStateService _gameStateService;
        private readonly IPlayerBlockerService _blockers;
        private readonly ISubscriber<EyesStateChangedMessage> _eyesStateChangedSubscriber;
        
        private readonly CompositeDisposable _disposable = new();
        
        [Inject]
        private EyesStateProcessor(IGameStateService gameStateService,
            IPlayerBlockerService blockers,
            ISubscriber<EyesStateChangedMessage>  eyesStateChangedSubscriber)
        {
            _gameStateService = gameStateService;
            _blockers = blockers;
            _eyesStateChangedSubscriber = eyesStateChangedSubscriber;
        }
        
        void IStartable.Start()
        {
            _eyesStateChangedSubscriber.Subscribe(message =>
            {
                if (message.IsClosed)
                {
                    _blockers.Add(PlayerBlocker.MovementBlock);
                    _gameStateService.Set(GameState.EyesClosed);
                }
                else
                {
                    _blockers.Remove(PlayerBlocker.MovementBlock);
                    _blockers.Remove(PlayerBlocker.LookBlock);
                    _gameStateService.Set(GameState.Gameplay);
                }
            }).AddTo(_disposable);
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}