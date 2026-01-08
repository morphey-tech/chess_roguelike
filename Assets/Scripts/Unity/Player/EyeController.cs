using System;
using System.Collections;
using DG.Tweening;
using MessagePipe;
using Project.Core.Character;
using Project.Core.Logging;
using Project.Gameplay.Gameplay.Player;
using UniRx;
using UnityEngine;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Unity.Player
{
    public sealed class EyeController : MonoBehaviour, IDisposable
    {
        [Header("UI элементы")]
        [SerializeField] private RectTransform _upperEyelid = null!;
        [SerializeField] private RectTransform _lowerEyelid = null!;
        [SerializeField] private CanvasGroup? _eyeOverlay;

        [Header("Настройки")]
        [SerializeField] private float _closeSpeed = 0.3f;
        [SerializeField] private float _openSpeed = 0.2f;
        [SerializeField] private float _blinkDuration = 0.15f;
        [SerializeField] private float _closedEyelidOffset = 0.5f;

        [Header("Состояния")]
        [SerializeField] private bool _startClosed;

        public IObservable<bool> OnEyeStateChanged => _eyeStateChanged;
        public bool IsEyesClosed { get; private set; }
        
        private IMovementCommandDispatcher _commandDispatcher = null!;
        private IPublisher<EyesStateChangedMessage> _publisher = null!;
        private readonly CompositeDisposable _disposables = new();
        private ILogger _logger = null!;

        private float _upperOpenY;
        private float _lowerOpenY;
        private float _upperClosedY;
        private float _lowerClosedY;

        private bool _isAnimating;
        private bool _disposed;

        private Tween? _currentTween;
        private readonly Subject<bool> _eyeStateChanged = new();

        [Inject]
        public void Construct(IMovementCommandDispatcher commandDispatcher,
            IPublisher<EyesStateChangedMessage>  publisher,
            ILogService logService)
        {
            _commandDispatcher = commandDispatcher;
            _publisher = publisher;
            _logger = logService.CreateLogger<EyeController>();
            SubscribeToInput();
        }

        private void Awake()
        {
            if (_eyeOverlay != null)
            {
                _eyeOverlay.alpha = 0f;
            }
        }

        private IEnumerator Start()
        {
            yield return null;
            InitializePositions();
            SetEyesClosed(_startClosed, instant: true);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void InitializePositions()
        {
            const float margin = 2f;

            float upperHeight = _upperEyelid.rect.height;
            _upperClosedY = 0f;
            _upperOpenY = upperHeight + margin;

            float lowerHeight = _lowerEyelid.rect.height;
            _lowerClosedY = 0f;
            _lowerOpenY = -lowerHeight - margin;
        }


        private void SubscribeToInput()
        {
            _commandDispatcher.OnEyeCloseChanged
                ?.Subscribe(closed => SetEyesClosed(closed))
                .AddTo(_disposables);
        }

        public void SetEyesClosed(bool closed, bool instant = false)
        {
            if (IsEyesClosed == closed && !instant)
            {
                return;
            }

            if (_isAnimating && !instant)
            {
                return;
            }

            IsEyesClosed = closed;
            _eyeStateChanged.OnNext(closed);
            _logger.Debug($"Eyes {(closed ? "closing" : "opening")}");

            if (instant)
            {
                KillTween();
                SetEyelidsInstant(closed);
            }
            else
            {
                AnimateEyelids(closed);
            }
        }

        public void Blink()
        {
            if (_isAnimating || IsEyesClosed)
            {
                return;
            }

            KillTween();
            _isAnimating = true;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_upperEyelid
                .DOAnchorPosY(_upperClosedY, _blinkDuration * 0.5f)
                .SetEase(Ease.InQuad));
            sequence.Join(_lowerEyelid
                .DOAnchorPosY(_lowerClosedY, _blinkDuration * 0.5f)
                .SetEase(Ease.InQuad));
            sequence.Append(_upperEyelid
                .DOAnchorPosY(_upperOpenY, _blinkDuration * 0.5f)
                .SetEase(Ease.OutQuad));
            sequence.Join(_lowerEyelid
                .DOAnchorPosY(_lowerOpenY, _blinkDuration * 0.5f)
                .SetEase(Ease.OutQuad));
            sequence.OnKill(() => _isAnimating = false);
            sequence.OnComplete(() => _isAnimating = false);

            _currentTween = sequence;
        }

        private void SetEyelidsInstant(bool closed)
        {
            float upperY = closed ? _upperClosedY : _upperOpenY;
            float lowerY = closed ? _lowerClosedY : _lowerOpenY;

            _upperEyelid.anchoredPosition =
                new Vector2(_upperEyelid.anchoredPosition.x, upperY);
            _lowerEyelid.anchoredPosition =
                new Vector2(_lowerEyelid.anchoredPosition.x, lowerY);

            if (_eyeOverlay != null)
            {
                _eyeOverlay.alpha = closed ? 1f : 0f;
            }
        }

        private void AnimateEyelids(bool closed)
        {
            KillTween();
            _isAnimating = true;
            _publisher.Publish(new EyesStateChangedMessage(closed));
            float duration = closed ? _closeSpeed : _openSpeed;
            float upperTargetY = closed ? _upperClosedY : _upperOpenY;
            float lowerTargetY = closed ? _lowerClosedY : _lowerOpenY;

            Ease ease = closed ? Ease.InQuad : Ease.OutQuad;
            Sequence sequence = DOTween.Sequence();

            sequence.Append(_upperEyelid
                .DOAnchorPosY(upperTargetY, duration)
                .SetEase(ease));
            sequence.Join(_lowerEyelid
                .DOAnchorPosY(lowerTargetY, duration)
                .SetEase(ease));

            if (_eyeOverlay != null)
            {
                sequence.Join(_eyeOverlay
                    .DOFade(closed ? 1f : 0f, duration));
            }

            sequence.OnKill(() => _isAnimating = false);
            sequence.OnComplete(() =>
            {
                _isAnimating = false;
                _logger.Debug($"Eyes {(closed ? "closed" : "opened")}");
            });
            _currentTween = sequence;
        }

        private void KillTween()
        {
            if (_currentTween == null)
            {
                return;
            }
            if (_currentTween.IsActive())
            {
                _currentTween.Kill();
            }
            _currentTween = null;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            KillTween();
            _eyeStateChanged.OnCompleted();
            _eyeStateChanged.Dispose();
            _disposables.Dispose();
        }
    }
}

