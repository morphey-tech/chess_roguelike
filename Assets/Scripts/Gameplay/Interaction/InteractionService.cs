using System;
using System.Collections.Generic;
using Project.Core.Config;
using Project.Core.Interaction;
using Project.Core.Logging;
using UniRx;
using UnityEngine;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Gameplay.Interaction
{
    public class InteractionService : IInteractionService, IDisposable
    {
        private readonly IConfigService _configService;
        private readonly ILogger _logger;

        private readonly Subject<IInteractable> _onTargetChanged = new();
        private readonly Subject<IInteractable> _onInteracted = new();
        private readonly List<InteractionCandidate> _candidates = new();

        private InteractionConfig _config;
        private Transform _playerTransform;
        private Transform _cameraTransform;
        private IInteractable _currentTarget;
        private float _lastUpdateTime;
        private bool _enabled = true;
        private bool _disposed;

        public IInteractable CurrentTarget => _currentTarget;
        public IObservable<IInteractable> OnTargetChanged => _onTargetChanged;
        public IObservable<IInteractable> OnInteracted => _onInteracted;

        [Inject]
        public InteractionService(IConfigService configService, ILogService logService)
        {
            _configService = configService;
            _logger = logService.CreateLogger<InteractionService>();
            _logger.Info("Initialized");
        }

        public void Initialize(Transform playerTransform, Transform cameraTransform)
        {
            _playerTransform = playerTransform;
            _cameraTransform = cameraTransform;

            if (_configService.TryGet<InteractionConfig>(out InteractionConfig config))
            {
                _config = config;
            }
            else
            {
                _config = new InteractionConfig();
                _logger.Warning("Using default InteractionConfig");
            }

            _logger.Debug("Player and camera references set");
        }

        public void Update()
        {
            if (!_enabled || _disposed) return;
            if (_playerTransform == null || _cameraTransform == null) return;
            if (Time.time - _lastUpdateTime < _config.UpdateInterval) return;

            _lastUpdateTime = Time.time;
            UpdateTarget();
        }

        private void UpdateTarget()
        {
            _candidates.Clear();

            Collider[] colliders = Physics.OverlapSphere(
                _playerTransform.position,
                _config.MaxDistance,
                _config.InteractableLayers,
                QueryTriggerInteraction.Collide
            );

            foreach (Collider col in colliders)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable == null || !interactable.CanInteract) continue;

                Vector3 targetPosition = col.bounds.center;
                float distance = Vector3.Distance(_playerTransform.position, targetPosition);

                if (distance < _config.MinDistance || distance > _config.MaxDistance) continue;

                Vector3 directionToTarget = (targetPosition - _cameraTransform.position).normalized;
                float angle = Vector3.Angle(_cameraTransform.forward, directionToTarget);

                if (angle > _config.MaxAngle) continue;

                if (_config.CheckLineOfSight)
                {
                    if (Physics.Raycast(
                        _cameraTransform.position,
                        directionToTarget,
                        out RaycastHit hit,
                        distance,
                        _config.ObstacleLayers,
                        QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider != col) continue;
                    }
                }

                float score = CalculateScore(distance, angle);
                _candidates.Add(new InteractionCandidate(interactable, distance, angle, targetPosition, score));
            }

            IInteractable bestTarget = null;
            float bestScore = float.MinValue;

            foreach (InteractionCandidate candidate in _candidates)
            {
                if (candidate.Score > bestScore)
                {
                    bestScore = candidate.Score;
                    bestTarget = candidate.Interactable;
                }
            }

            SetTarget(bestTarget);
        }

        private float CalculateScore(float distance, float angle)
        {
            float normalizedDistance = 1f - (distance - _config.MinDistance) / (_config.MaxDistance - _config.MinDistance);
            float normalizedAngle = 1f - angle / _config.MaxAngle;

            float distanceScore = _config.PreferCloser ? normalizedDistance : 0.5f;
            float angleScore = _config.PreferCentered ? normalizedAngle : 0.5f;

            return distanceScore * _config.DistanceWeight + angleScore * _config.AngleWeight;
        }

        private void SetTarget(IInteractable newTarget)
        {
            if (_currentTarget == newTarget) return;

            IInteractable previousTarget = _currentTarget;

            previousTarget?.OnUnfocused();
            _currentTarget = newTarget;
            _currentTarget?.OnFocused();

            _logger.Trace($"Target changed: {previousTarget?.InteractionId ?? "null"} -> {_currentTarget?.InteractionId ?? "null"}");
            _onTargetChanged.OnNext(_currentTarget);
        }

        public void TryInteract()
        {
            if (!_enabled || _currentTarget == null) return;
            if (!_currentTarget.CanInteract) return;

            _logger.Info($"Interacting with: {_currentTarget.InteractionId}");
            _currentTarget.Interact();
            _onInteracted.OnNext(_currentTarget);
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;

            if (!enabled)
            {
                SetTarget(null);
            }

            _logger.Debug($"Enabled: {enabled}");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _onTargetChanged.Dispose();
            _onInteracted.Dispose();
            _candidates.Clear();

            _logger.Info("Disposed");
        }
    }
}
