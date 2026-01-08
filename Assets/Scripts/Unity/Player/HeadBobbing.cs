using System;
using Project.Core.Character;
using UniRx;
using UnityEngine;
using VContainer;

namespace Project.Unity.Player
{
    public class HeadBobbing : MonoBehaviour, IDisposable
    {
        [Header("Ходьба")]
        [SerializeField] private float _walkBobSpeed = 10f;
        [SerializeField] private float _walkBobAmount = 0.03f;
        
        [Header("Бег")]
        [SerializeField] private float _sprintBobSpeed = 14f;
        [SerializeField] private float _sprintBobAmount = 0.05f;
        
        [Header("Присед")]
        [SerializeField] private float _crouchBobSpeed = 6f;
        [SerializeField] private float _crouchBobAmount = 0.02f;
        
        [Header("Дыхание")]
        [SerializeField] private float _breathingSpeed = 2f;
        [SerializeField] private float _breathingAmount = 0.005f;
        
        [Header("Сглаживание")]
        [SerializeField] private float _smoothing = 10f;
        
        private IMovementCommandDispatcher _commandDispatcher;
        private Character.CharacterMotor _motor;
        private readonly CompositeDisposable _disposables = new();
        
        private Vector3 _originalLocalPosition;
        private float _bobTimer;
        private float _targetBobOffset;
        private float _currentBobOffset;
        private MovementCommand _lastCommand;
        private bool _disposed;
        
        private bool _isInjected;
        
        [Inject]
        public void Construct(IMovementCommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _isInjected = true;
            
            _commandDispatcher?.OnMovement
                .Subscribe(cmd => _lastCommand = cmd)
                .AddTo(_disposables);
        }
        
        private void Awake()
        {
            _motor = GetComponentInParent<Character.CharacterMotor>();
        }
        
        private void Start()
        {
            _originalLocalPosition = transform.localPosition;
        }
        
        private void LateUpdate()
        {
            if (_motor == null || !_isInjected) return;
            
            UpdateBobbing();
            ApplyBobbing();
        }
        
        private void UpdateBobbing()
        {
            bool isMoving = _lastCommand.Direction.sqrMagnitude > 0.1f;
            bool isGrounded = _motor.Grounded;
            CharacterState state = _motor.State;
            
            if (state == CharacterState.OnLadder)
            {
                float climbInput = Mathf.Abs(_lastCommand.Direction.y);
                if (climbInput > 0.1f)
                {
                    _bobTimer += Time.deltaTime * _walkBobSpeed;
                    _targetBobOffset = Mathf.Sin(_bobTimer) * _walkBobAmount * 0.5f;
                }
                else
                {
                    _targetBobOffset = Mathf.Sin(Time.time * _breathingSpeed) * _breathingAmount;
                }
            }
            else if (isMoving && isGrounded && state != CharacterState.InAir)
            {
                float bobSpeed;
                float bobAmount;
                
                if (state == CharacterState.Crouching)
                {
                    bobSpeed = _crouchBobSpeed;
                    bobAmount = _crouchBobAmount;
                }
                else if (_lastCommand.IsSprinting)
                {
                    bobSpeed = _sprintBobSpeed;
                    bobAmount = _sprintBobAmount;
                }
                else
                {
                    bobSpeed = _walkBobSpeed;
                    bobAmount = _walkBobAmount;
                }
                
                _bobTimer += Time.deltaTime * bobSpeed;
                _targetBobOffset = Mathf.Sin(_bobTimer) * bobAmount;
            }
            else
            {
                _bobTimer = 0f;
                _targetBobOffset = Mathf.Sin(Time.time * _breathingSpeed) * _breathingAmount;
            }
        }
        
        private void ApplyBobbing()
        {
            _currentBobOffset = Mathf.Lerp(_currentBobOffset, _targetBobOffset, Time.deltaTime * _smoothing);
            
            Vector3 targetPosition = _originalLocalPosition + Vector3.up * _currentBobOffset;
            transform.localPosition = targetPosition;
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _disposables?.Dispose();
        }
    }
}


