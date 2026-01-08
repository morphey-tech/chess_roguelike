using System;
using Project.Core.Character;
using Project.Core.Interaction;
using Project.Gameplay.Interaction;
using UniRx;
using UnityEngine;
using VContainer;

namespace Project.Unity.Interaction
{
    public class PlayerInteractionHandler : MonoBehaviour, IDisposable
    {
        [SerializeField] private Transform _cameraTransform;
        
        private InteractionService _interactionService;
        private IMovementCommandDispatcher _commandDispatcher;
        private CompositeDisposable _disposables = new();
        private bool _disposed;
        
        [Inject]
        public void Construct(InteractionService interactionService, IMovementCommandDispatcher commandDispatcher)
        {
            _interactionService = interactionService;
            _commandDispatcher = commandDispatcher;
            
            // Inject вызывается ПОСЛЕ Start(), инициализируем здесь
            _interactionService.Initialize(transform, _cameraTransform);
            
            _commandDispatcher.OnInteract
                .Subscribe(_ => _interactionService.TryInteract())
                .AddTo(_disposables);
        }
        
        private void Start()
        {
            // Инициализация перенесена в Construct
        }
        
        private void Update()
        {
            _interactionService.Update();
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _disposables?.Dispose();
        }
    }
}


