using System;
using Project.Core.Character;
using Project.Core.Logging;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Gameplay.Character
{
    public class MovementCommandDispatcher : IMovementCommandDispatcher, IDisposable
    {
        private readonly Subject<MovementCommand> _onMovement = new();
        private readonly Subject<JumpCommand> _onJump = new();
        private readonly Subject<InteractCommand> _onInteract = new();
        private readonly Subject<LookCommand> _onLook = new();
        private readonly Subject<bool> _onCrouchChanged = new();
        private readonly Subject<bool> _onEyeCloseChanged = new();
        
        private readonly InputAction _moveAction;
        private readonly InputAction _lookAction;
        private readonly InputAction _jumpAction;
        private readonly InputAction _sprintAction;
        private readonly InputAction _crouchAction;
        private readonly InputAction _interactAction;
        private readonly InputAction _eyeCloseAction;
        
        private readonly CompositeDisposable _disposables = new();
        private readonly ILogger _logger;
        
        private bool _isSprinting;
        private bool _isCrouching;
        private bool _disposed;
        
        public IObservable<MovementCommand> OnMovement => _onMovement;
        public IObservable<JumpCommand> OnJump => _onJump;
        public IObservable<InteractCommand> OnInteract => _onInteract;
        public IObservable<LookCommand> OnLook => _onLook;
        public IObservable<bool> OnCrouchChanged => _onCrouchChanged;
        public IObservable<bool> OnEyeCloseChanged => _onEyeCloseChanged;
        
        [Inject]
        public MovementCommandDispatcher(InputActionAsset inputActions, ILogService logService)
        {
            _logger = logService.CreateLogger<MovementCommandDispatcher>();
            
            InputActionMap playerMap = inputActions.FindActionMap("Player");
            
            _moveAction = playerMap.FindAction("Move");
            _lookAction = playerMap.FindAction("Look");
            _jumpAction = playerMap.FindAction("Jump");
            _sprintAction = playerMap.FindAction("Sprint");
            _crouchAction = playerMap.FindAction("Crouch");
            _interactAction = playerMap.FindAction("Interact");
            _eyeCloseAction = playerMap.FindAction("EyeClose");
            
            SetupBindings();
            EnableInput();
            
            _logger.Info("Initialized");
        }
        
        private void SetupBindings()
        {
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    Vector2 moveDir = _moveAction.ReadValue<Vector2>();
                    Vector2 lookDelta = _lookAction.ReadValue<Vector2>();
                    
                    _onMovement.OnNext(new MovementCommand(moveDir, _isSprinting, _isCrouching));
                    
                    if (lookDelta.sqrMagnitude > 0.01f)
                    {
                        _onLook.OnNext(new LookCommand(lookDelta));
                    }
                })
                .AddTo(_disposables);
            
            Observable.FromEvent<InputAction.CallbackContext>(
                    h => _jumpAction.performed += h,
                    h => _jumpAction.performed -= h)
                .Subscribe(_ => _onJump.OnNext(JumpCommand.Default))
                .AddTo(_disposables);
            
            Observable.FromEvent<InputAction.CallbackContext>(
                    h => _sprintAction.performed += h,
                    h => _sprintAction.performed -= h)
                .Subscribe(_ => _isSprinting = true)
                .AddTo(_disposables);
            
            Observable.FromEvent<InputAction.CallbackContext>(
                    h => _sprintAction.canceled += h,
                    h => _sprintAction.canceled -= h)
                .Subscribe(_ => _isSprinting = false)
                .AddTo(_disposables);
            
            Observable.FromEvent<InputAction.CallbackContext>(
                    h => _crouchAction.performed += h,
                    h => _crouchAction.performed -= h)
                .Subscribe(_ =>
                {
                    _isCrouching = true;
                    _onCrouchChanged.OnNext(true);
                })
                .AddTo(_disposables);
            
            Observable.FromEvent<InputAction.CallbackContext>(
                    h => _crouchAction.canceled += h,
                    h => _crouchAction.canceled -= h)
                .Subscribe(_ =>
                {
                    _isCrouching = false;
                    _onCrouchChanged.OnNext(false);
                })
                .AddTo(_disposables);
            
            Observable.FromEvent<InputAction.CallbackContext>(
                    h => _interactAction.performed += h,
                    h => _interactAction.performed -= h)
                .Subscribe(_ => _onInteract.OnNext(InteractCommand.Default))
                .AddTo(_disposables);
            
            // Закрытие глаз (удержание)
            if (_eyeCloseAction != null)
            {
                Observable.EveryUpdate()
                    .Select(_ => _eyeCloseAction.IsPressed())
                    .DistinctUntilChanged()
                    .Subscribe(isPressed =>
                    {
                        _onEyeCloseChanged.OnNext(isPressed);
                    })
                    .AddTo(_disposables);
            }
        }
        
        public void EnableInput()
        {
            _moveAction?.Enable();
            _lookAction?.Enable();
            _jumpAction?.Enable();
            _sprintAction?.Enable();
            _crouchAction?.Enable();
            _interactAction?.Enable();
            _eyeCloseAction?.Enable();
            _logger.Debug("Input enabled");
        }
        
        public void DisableInput()
        {
            _moveAction?.Disable();
            _lookAction?.Disable();
            _jumpAction?.Disable();
            _sprintAction?.Disable();
            _crouchAction?.Disable();
            _interactAction?.Disable();
            _eyeCloseAction?.Disable();
            _logger.Debug("Input disabled");
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _logger.Info("Disposing");
            
            DisableInput();
            _disposables.Dispose();
            _onMovement.Dispose();
            _onJump.Dispose();
            _onInteract.Dispose();
            _onLook.Dispose();
            _onCrouchChanged.Dispose();
            _onEyeCloseChanged.Dispose();
        }
    }
}


