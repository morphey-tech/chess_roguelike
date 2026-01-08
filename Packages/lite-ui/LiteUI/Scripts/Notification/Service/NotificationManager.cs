using System;
using System.Collections.Generic;
using System.Threading;
using LiteUI.Common.Logger;
using Cysharp.Threading.Tasks;
using LiteUI.Common.Extensions;
using LiteUI.Notification.Controller;
using LiteUI.Notification.Model;
using LiteUI.UI.Model;
using LiteUI.UI.Service;
using UnityEngine;
using VContainer;

namespace LiteUI.Notification.Service
{
    public sealed class NotificationManager : IDisposable
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<NotificationManager>();

        private UIService _uiService = null!;
        private NotificationQueueService _queueService = null!;
        private ScreenLayout _screenLayout = null!;

        private int _mutedAllCount;
        private GameObject _rootContainer = null!;
        private NotificationModel? _currentModel;
        private UINotification? _currentNotification;
        private readonly List<string> _mutedTags = new();

        private readonly CancellationTokenSource _disposeCancellationTokenSource = new();

        [Inject]
        private void Construct(UIService uiService, NotificationQueueService notificationQueueService, ScreenLayout screenLayout)
        {
            _uiService = uiService;
            _queueService = notificationQueueService;
            _screenLayout = screenLayout;
        }

        void IDisposable.Dispose()
        {
            _disposeCancellationTokenSource.Cancel();
            _disposeCancellationTokenSource.Dispose();
        }

        public void AttachRootContainer(GameObject rootContainer)
        {
            _rootContainer = rootContainer;
        }

        public void ContinueQueue()
        {
            TryShowNext(_disposeCancellationTokenSource.Token).Forget();
        }

        public string Show(NotificationModel model)
        {
            _queueService.AddLast(model);
            if (_currentModel == null) {
                TryShowNext(_disposeCancellationTokenSource.Token).Forget();
            }
            return model.Id;
        }

        public void HideById(string id)
        {
            _queueService.RemoveById(id);
            if (_currentModel?.Id != id) {
                return;
            }
            if (_currentNotification != null) {
                Hide(_currentNotification.MonoBehaviour);
            } else {
                _currentModel = null;
            }
        }
        
        public void HideByTag(string tag)
        {
            _queueService.RemoveByTag(tag);
            if (_currentModel == null || !_currentModel.HasTag(tag)) {
                return;
            }
            if (_currentNotification != null) {
                Hide(_currentNotification.MonoBehaviour);
            } else {
                _currentModel = null;
            }
        }

        public void Hide(MonoBehaviour monoBehaviour)
        {
            if (_currentNotification == null) {
                _logger.Error("Current notification is null");
                Release(monoBehaviour);
                return;
            }
            if (_currentNotification.MonoBehaviour != monoBehaviour) {
                _logger.Error($"Current notification is different from requested to hide notification, notification={monoBehaviour.name}, "
                              + $"currentNotification={_currentNotification.MonoBehaviour.name}");
                Release(monoBehaviour);
                return;
            }
            _currentNotification.Hide().Forget();
        }

        public bool IsShowingById(string id)
        {
            return _currentModel?.Id == id;
        }

        public bool IsShowingByTag(string tag)
        {
            return _currentModel != null && _currentModel.HasTag(tag);
        }

        public bool IsQueuedById(string id)
        {
            return _queueService.ContainsById(id);
        }
        
        public bool IsQueuedByTag(string tag)
        {
            return _queueService.ContainsByTag(tag);
        }

        public bool IsOngoingById(string id)
        {
            return IsShowingById(id) || IsQueuedById(id);
        }
        
        public bool IsOngoingByTag(string tag)
        {
            return IsShowingByTag(tag) || IsQueuedByTag(tag);
        }

        public void MuteByTag(string tag)
        {
            _mutedTags.Add(tag);
            if (_currentModel == null || !_currentModel.HasTag(tag)) {
                return;
            }
            _queueService.AddFirst(_currentModel);
            _currentModel = null;
            if (_currentNotification != null) {
                Hide(_currentNotification.MonoBehaviour);
            }
        }

        public void MuteByTags(List<string> tags)
        {
            _mutedTags.AddRange(tags);
            if (_currentModel == null || !_currentModel.HasAnyTag(tags)) {
                return;
            }
            _queueService.AddFirst(_currentModel);
            _currentModel = null;
            if (_currentNotification != null) {
                Hide(_currentNotification.MonoBehaviour);
            }
        }

        public void MuteAll()
        {
            _mutedAllCount++;
            if (_currentModel == null) {
                return;
            }
            _queueService.AddFirst(_currentModel);
            _currentModel = null;
            if (_currentNotification != null) {
                Hide(_currentNotification.MonoBehaviour);
            }
        }

        public void UnmuteByTag(string tag)
        {
            if (_mutedTags.Contains(tag)) {
                _mutedTags.Remove(tag);
            }
            TryShowNext(_disposeCancellationTokenSource.Token).Forget();
        }

        public void UnmuteByTags(List<string> tags)
        {
            foreach (string tag in tags) {
                if (_mutedTags.Contains(tag)) {
                    _mutedTags.Remove(tag);
                }
            }
            TryShowNext(_disposeCancellationTokenSource.Token).Forget();
        }

        public void UnmuteAll()
        {
            if (_mutedAllCount - 1 < 0) {
                _logger.Error("Try to unmute when it is not muted");
                return;
            }
            _mutedAllCount--;
            TryShowNext(_disposeCancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid TryShowNext(CancellationToken cancellationToken)
        {
            if (_mutedAllCount > 0) {
                return;
            }
            if (_currentModel != null) {
                return;
            }
            NotificationModel? model = _queueService.Pull(_mutedTags);

            if (model == null) {
                return;
            }
            _currentModel = model;

            try {
                await ShowAsync(model, cancellationToken);
            } catch (OperationCanceledException) {
                // Nothing
            } catch (Exception e) {
                _logger.Error("Showing notification error", e);
            } finally {
                _currentModel = null;
            }
            cancellationToken.ThrowIfCancellationRequested();

            TryShowNext(cancellationToken).Forget();
        }

        private async UniTask ShowAsync(NotificationModel model, CancellationToken cancellationToken)
        {
            MonoBehaviour monoBehaviour = await Create(model, cancellationToken);
            if (_currentModel != model) {
                Release(monoBehaviour);
                return;
            }
            UINotification notification = new(model, monoBehaviour, _screenLayout);
            _currentNotification = notification;
            try {
                await notification.Show();
                cancellationToken.ThrowIfCancellationRequested();
            } finally {
                try {
                    await notification.Hide();
                    cancellationToken.ThrowIfCancellationRequested();
                } finally {
                    if (!monoBehaviour.IsDestroyed()) {
                        Release(monoBehaviour);
                    }
                }
            }
        }

        private async UniTask<MonoBehaviour> Create(NotificationModel model, CancellationToken cancellationToken)
        {
            MonoBehaviour monoBehaviour = await _uiService.CreateAsync(UIModel.Create(model.Type, model.Parameters).Container(_rootContainer),
                                                                       cancellationToken);
            monoBehaviour.name = model.Id;
            return monoBehaviour;
        }

        private void Release(MonoBehaviour monoBehaviour)
        {
            _uiService.Release(monoBehaviour.gameObject);
        }

        public bool IsShowing => _currentModel != null;
    }
}
