using System;
using MessagePipe;
using Project.Core.Character;
using Project.Core.Logging;
using Project.Core.Messages;
using Project.Core.Player;
using Project.Unity.Character;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Unity.Player
{
    public class PlayerMotor : CharacterMotor, IDisposable
    {
        [Header("Камера")]
        [SerializeField] private Transform _cameraRig;      // Высота (приседание)
        [SerializeField] private Transform _cameraPivot;    // Rotation (pitch вверх/вниз)
        [SerializeField] private Transform _model;          // 3D модель персонажа

        [Header("Настройки камеры")]
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _minLookAngle = -89f;
        [SerializeField] private float _maxLookAngle = 89f;
        [SerializeField] private float _standingCameraHeight = 1.6f;
        [SerializeField] private float _crouchingCameraHeight = 0.9f;

        [Header("Синхронизация")]
        [SerializeField] private float _modelSyncInterval = 0.5f;

        private IMovementCommandDispatcher _commandDispatcher = null!;
        private IPlayerControlState _controlState = null!;
        private IPublisher<CharacterStateChangedMessage> _stateChangedPublisher;
        private IPublisher<CharacterLandedMessage> _landedPublisher;
        private IPublisher<LadderInteractionMessage> _ladderPublisher;
        private PlayerModel _playerModel;
        private ILogger _logger;

        private CompositeDisposable _disposables = new();

        private MovementCommand _lastMovementCommand;
        private LookCommand _pendingLook;
        private float _verticalLookAngle;
        private float _currentCameraHeight;
        private float _ladderDirection;
        private float _lastSyncTime;
        private bool _disposed;
        private bool _isInjected;
        private bool _hasLookInput;

        [Inject]
        public void Construct(
            CharacterMovementSettings settings,
            IMovementCommandDispatcher commandDispatcher,
            IPlayerControlState controlState,
            PlayerModel playerModel,
            ILogService logService,
            IPublisher<CharacterStateChangedMessage> stateChangedPublisher,
            IPublisher<CharacterLandedMessage> landedPublisher,
            IPublisher<LadderInteractionMessage> ladderPublisher)
        {
            Settings = settings;
            _commandDispatcher = commandDispatcher;
            _controlState = controlState;
            _playerModel = playerModel;
            _logger = logService.CreateLogger<PlayerMotor>();
            _stateChangedPublisher = stateChangedPublisher;
            _landedPublisher = landedPublisher;
            _ladderPublisher = ladderPublisher;
            _isInjected = true;
            SubscribeToCommands();
            _logger.Info("Initialized with DI");
        }

        protected override void Start()
        {
            base.Start();
            _currentCameraHeight = _standingCameraHeight;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void SubscribeToCommands()
        {
            _commandDispatcher.OnMovement
                .Subscribe(cmd =>
                {
                    _lastMovementCommand = cmd;
                    _ladderDirection = cmd.Direction.y;
                })
                .AddTo(_disposables);

            _commandDispatcher.OnJump
                .Subscribe(_ =>
                {
                    if (CurrentState == CharacterState.OnLadder)
                    {
                        JumpFromLadder();
                    }
                    else
                    {
                        TryJump();
                    }
                })
                .AddTo(_disposables);

            _commandDispatcher.OnCrouchChanged
                .Subscribe(SetCrouchTarget)
                .AddTo(_disposables);

            _commandDispatcher.OnLook
                .Subscribe(cmd =>
                {
                    _pendingLook = cmd;
                    _hasLookInput = true;
                })
                .AddTo(_disposables);

            _commandDispatcher.OnInteract
                .Subscribe(_ => TryInteract())
                .AddTo(_disposables);
        }

        protected override void Update()
        {
            if (!CanMovement())
            {
                return;
            }
            
            base.Update();

            if (!_isInjected)
            {
                HandleDirectInput();
            }

            UpdateCameraPosition();
            SyncToModel();
        }

        protected override void FixedUpdate()
        {
            if (!CanMovement())
            {
                StopMovement();
                return;
            }
            base.FixedUpdate();
        }

        private void LateUpdate()
        {
            if (!CanLook())
            {
                return;
            }
            if (!_hasLookInput)
            {
                return;
            }
            ApplyLook(_pendingLook);
            _hasLookInput = false;
        }

        private void HandleDirectInput()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (keyboard == null || mouse == null)
            {
                return;
            }

            Vector2 moveInput = Vector2.zero;
            if (keyboard.wKey.isPressed) moveInput.y += 1;
            if (keyboard.sKey.isPressed) moveInput.y -= 1;
            if (keyboard.aKey.isPressed) moveInput.x -= 1;
            if (keyboard.dKey.isPressed) moveInput.x += 1;

            bool isSprinting = keyboard.leftShiftKey.isPressed;
            bool isCrouching = keyboard.cKey.isPressed;

            _lastMovementCommand = new MovementCommand(moveInput, isSprinting, isCrouching);
            _ladderDirection = moveInput.y;

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                if (CurrentState == CharacterState.OnLadder)
                {
                    JumpFromLadder();
                }
                else
                {
                    TryJump();
                }
            }

            Vector2 lookDelta = mouse.delta.ReadValue();
            if (lookDelta.sqrMagnitude > 0.01f)
            {
                _pendingLook = new LookCommand(lookDelta);
                _hasLookInput = true;
            }

            if (keyboard.eKey.wasPressedThisFrame)
            {
                TryInteract();
            }

            if (keyboard.cKey.wasPressedThisFrame)
            {
                SetCrouchTarget(true);
            }
            else if (keyboard.cKey.wasReleasedThisFrame)
            {
                SetCrouchTarget(false);
            }
        }

        private void SyncToModel()
        {
            if (_playerModel == null)
            {
                return;
            }

            if (Time.time - _lastSyncTime < _modelSyncInterval)
            {
                return;
            }

            _lastSyncTime = Time.time;
            _playerModel.SetPosition(transform.position);
            _playerModel.SetRotation(transform.eulerAngles.y);
        }

        protected override void ProcessGroundMovement()
        {
            ApplyGroundMovement(_lastMovementCommand.Direction, _lastMovementCommand.IsSprinting);
        }

        protected override void ProcessAirMovement()
        {
            ApplyAirMovement(_lastMovementCommand.Direction, _lastMovementCommand.IsSprinting);
        }

        protected override void ProcessLadderMovement()
        {
            ProcessLadderClimb(_ladderDirection);
        }

        protected override bool GetTargetCrouchState()
        {
            return _lastMovementCommand.IsCrouching;
        }

        protected override void StopMovement()
        {
            _lastMovementCommand = default;
            Velocity.x = 0f;
            Velocity.z = 0f;
        }

        private void ApplyLook(LookCommand cmd)
        {
            Vector2 lookInput = cmd.Delta * _mouseSensitivity;

            // Yaw (горизонтальный поворот) - вращаем весь персонаж
            transform.Rotate(Vector3.up, lookInput.x);

            // Pitch (вертикальный поворот) - только камера
            _verticalLookAngle -= lookInput.y;
            _verticalLookAngle = Mathf.Clamp(_verticalLookAngle, _minLookAngle, _maxLookAngle);

            if (_cameraPivot != null)
            {
                _cameraPivot.localRotation = Quaternion.Euler(_verticalLookAngle, 0f, 0f);
            }
        }

        private void UpdateCameraPosition()
        {
            if (_cameraRig == null || Settings == null)
            {
                return;
            }

            float targetCameraHeight = CurrentState == CharacterState.Crouching
                ? _crouchingCameraHeight
                : _standingCameraHeight;

            _currentCameraHeight = Mathf.MoveTowards(
                _currentCameraHeight,
                targetCameraHeight,
                Settings.CrouchTransitionSpeed * Time.deltaTime
            );

            // CameraRig двигается по высоте относительно центра капсулы
            _cameraRig.localPosition = Vector3.up * _currentCameraHeight;
        }

        private void TryInteract()
        {
            if (_cameraPivot == null || Settings == null)
            {
                return;
            }

            // Рейкаст из позиции камеры в направлении взгляда
            if (Physics.Raycast(
                _cameraPivot.position,
                _cameraPivot.forward,
                out RaycastHit hit,
                2f,
                Settings.GroundLayers,
                QueryTriggerInteraction.Collide))
            {
                Core.Ladder.ILadder ladder = hit.collider.GetComponent<Core.Ladder.ILadder>();
                if (ladder != null)
                {
                    EnterLadder(ladder);
                }
            }
        }

        private bool CanMovement()
        {
            return _isInjected && _controlState.CanMove.Value;
        }

        private bool CanLook()
        {
            return _isInjected && _controlState.CanLook.Value;
        }

        protected override void OnStateChanged(CharacterState previousState, CharacterState newState)
        {
            _logger?.Debug($"State: {previousState} -> {newState}");
            _stateChangedPublisher?.Publish(new CharacterStateChangedMessage(previousState, newState));
        }

        protected override void OnLanded(float fallSpeed)
        {
            _logger?.Trace($"Landed with speed: {fallSpeed}");
            _landedPublisher?.Publish(new CharacterLandedMessage(fallSpeed));
        }

        protected override void OnLadderEntered()
        {
            _logger?.Debug("Entered ladder");
            _ladderPublisher?.Publish(new LadderInteractionMessage(true));
        }

        protected override void OnLadderExited()
        {
            _logger?.Debug("Exited ladder");
            _ladderPublisher?.Publish(new LadderInteractionMessage(false));
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
            _logger?.Info("Disposed");
        }
    }
}
