using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Messages;
using UnityEngine;
using VContainer;

namespace Project.Unity.Player
{
    public class CameraShake : MonoBehaviour, IDisposable
    {
        [Header("Настройки")]
        [SerializeField] private float _maxShakeAmount = 0.3f;
        [SerializeField] private float _shakeDuration = 0.2f;
        [SerializeField] private float _shakeFrequency = 25f;
        [SerializeField] private AnimationCurve _shakeFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Приземление")]
        [SerializeField] private float _landingShakeMultiplier = 0.02f;
        [SerializeField] private float _minFallSpeedForShake = 5f;
        
        private ISubscriber<CharacterLandedMessage> _landedSubscriber;
        private IDisposable _subscription;
        
        private Vector3 _originalLocalPosition;
        private float _currentShakeAmount;
        private float _currentShakeTime;
        private bool _isShaking;
        private bool _disposed;
        
        [Inject]
        public void Construct(ISubscriber<CharacterLandedMessage> landedSubscriber)
        {
            _landedSubscriber = landedSubscriber;
        }
        
        private void Awake()
        {
            _originalLocalPosition = transform.localPosition;
        }
        
        private void Start()
        {
            _subscription = _landedSubscriber?.Subscribe(OnCharacterLanded);
        }
        
        private void OnCharacterLanded(CharacterLandedMessage message)
        {
            if (message.FallSpeed > _minFallSpeedForShake)
            {
                float intensity = (message.FallSpeed - _minFallSpeedForShake) * _landingShakeMultiplier;
                Shake(Mathf.Min(intensity, 1f));
            }
        }
        
        private void Update()
        {
            if (!_isShaking) return;
            
            _currentShakeTime += Time.deltaTime;
            
            if (_currentShakeTime >= _shakeDuration)
            {
                _isShaking = false;
                transform.localPosition = _originalLocalPosition;
                return;
            }
            
            float progress = _currentShakeTime / _shakeDuration;
            float falloff = _shakeFalloff.Evaluate(progress);
            float amount = _currentShakeAmount * falloff;
            
            Vector3 offset = new Vector3(
                Mathf.PerlinNoise(Time.time * _shakeFrequency, 0) * 2 - 1,
                Mathf.PerlinNoise(0, Time.time * _shakeFrequency) * 2 - 1,
                0
            ) * amount;
            
            transform.localPosition = _originalLocalPosition + offset;
        }
        
        public void Shake(float intensity)
        {
            _currentShakeAmount = Mathf.Clamp01(intensity) * _maxShakeAmount;
            _currentShakeTime = 0f;
            _isShaking = true;
        }
        
        public async UniTaskVoid ShakeAsync(float intensity, float duration)
        {
            _currentShakeAmount = Mathf.Clamp01(intensity) * _maxShakeAmount;
            _currentShakeTime = 0f;
            
            float originalDuration = _shakeDuration;
            _shakeDuration = duration;
            _isShaking = true;
            
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: destroyCancellationToken);
            
            _shakeDuration = originalDuration;
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _subscription?.Dispose();
        }
    }
}


