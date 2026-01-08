using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LiteUI.Common.Extensions;
using LiteUI.Common.Model;
using LiteUI.Notification.Model;
using LiteUI.Tweening;
using LiteUI.UI.Service;
using UnityEngine;

namespace LiteUI.Notification.Controller
{
    public class UINotification : INotification
    {
        private static readonly Vector2 TOP_PIVOT = new Vector2(0.5f, 1);
        private const string OPEN_ID = "Open";

        public NotificationModel Model { get; }
        public MonoBehaviour MonoBehaviour { get; }

        private CancellationTokenSource? _showTokenSource;

        private UniTaskCompletionSource? _showCompletionSource;
        private UniTaskCompletionSource? _hideCompletionSource;

        private readonly ScreenLayout _screenLayout;

        public UINotification(NotificationModel model, MonoBehaviour monoBehaviour, ScreenLayout screenLayout)
        {
            Model = model;
            MonoBehaviour = monoBehaviour;
            _screenLayout = screenLayout;
        }

        public async UniTask Show()
        {
            if (MonoBehaviour.IsDestroyed()) {
                return;
            }
            if (_hideCompletionSource != null) {
                return;
            }
            if (_showCompletionSource != null) {
                if (_showCompletionSource.UnsafeGetStatus() == UniTaskStatus.Pending) {
                    await _showCompletionSource.Task;
                }
                return;
            }

            float topOffset = _screenLayout.GetOffset(Direction.UP);
            RectTransform rectTransform = (RectTransform) MonoBehaviour.transform;
            rectTransform.pivot = TOP_PIVOT;
            rectTransform.anchorMin = TOP_PIVOT;
            rectTransform.anchorMax = TOP_PIVOT;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.sizeDelta.y - topOffset);
            GameObject.SetActive(true);

            _showCompletionSource = new UniTaskCompletionSource();
            CancellationTokenSource showTokenSource = new();
            _showTokenSource = showTokenSource;
            CancellationTokenSource linkedTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(DestroyCancellationToken, showTokenSource.Token);

            try {
                await GameObject.DOAnimationPlayForwardByIdAsync(OPEN_ID, forceRewind: true, cancellationToken: linkedTokenSource.Token);
                await UniTask.Delay(TimeSpan.FromSeconds(Model.Duration), cancellationToken: linkedTokenSource.Token);
                _showCompletionSource.TrySetResult();
            } catch (OperationCanceledException) {
                _showCompletionSource.TrySetCanceled(linkedTokenSource.Token);
            } catch (Exception e) {
                _showCompletionSource.TrySetException(e);
            }
            linkedTokenSource.Dispose();
            showTokenSource.Dispose();
            if (_showTokenSource == showTokenSource) {
                _showTokenSource = null;
            }

            await _showCompletionSource.Task;
        }

        public async UniTask Hide()
        {
            if (MonoBehaviour.IsDestroyed()) {
                return;
            }
            if (_hideCompletionSource != null) {
                if (_hideCompletionSource.UnsafeGetStatus() == UniTaskStatus.Pending) {
                    await _hideCompletionSource.Task;
                }
                return;
            }

            _hideCompletionSource = new UniTaskCompletionSource();
            _showTokenSource?.Cancel();

            try {
                await GameObject.DOAnimationPlayBackwardsByIdAsync(OPEN_ID, cancellationToken: DestroyCancellationToken);
                GameObject.SetActive(false);
                _hideCompletionSource.TrySetResult();
            } catch (OperationCanceledException) {
                _hideCompletionSource.TrySetCanceled(DestroyCancellationToken);
            } catch (Exception e) {
                _hideCompletionSource.TrySetException(e);
            }

            await _hideCompletionSource.Task;
        }

        public bool IsShowing => _showCompletionSource != null && _showCompletionSource.UnsafeGetStatus() == UniTaskStatus.Pending;
        
        public bool IsHiding => _hideCompletionSource != null && _hideCompletionSource.UnsafeGetStatus() == UniTaskStatus.Pending;

        private GameObject GameObject => MonoBehaviour.gameObject;

        private CancellationToken DestroyCancellationToken => MonoBehaviour.destroyCancellationToken;
    }
}
