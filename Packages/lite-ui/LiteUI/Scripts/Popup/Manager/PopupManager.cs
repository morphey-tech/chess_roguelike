using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LiteUI.Common.Extensions;
using LiteUI.Popup.Model;
using LiteUI.Popup.Panel;
using LiteUI.UI.Service;
using UnityEngine;
using VContainer;

namespace LiteUI.Popup.Manager
{
    public sealed class PopupManager : IDisposable
    {
        private IPopupLoader _popupLoader = null!;

        private GameObject _popupContainer = null!;
        private int _count;
        private string? _currentId;
        private IPopup? _currentPopup;
        private readonly Dictionary<string, IPopup?> _hidingItems = new();

        private CancellationTokenSource? _loadCancellationTokenSource;

        [Inject]
        public void Construct(IPopupLoader popupLoader, ScreenLayout screenLayout)
        {
            _popupLoader = popupLoader;
        }

        void IDisposable.Dispose()
        {
            _loadCancellationTokenSource?.Dispose();
            _loadCancellationTokenSource = null;
        }

        public void AttachRootContainer(GameObject root)
        {
            _popupContainer = root;
        }

        public string Show<T>(RectTransform target, PopupAlign defaultAlign, Vector2 offset, params object?[]? parameters)
                where T : MonoBehaviour, IPopup
        {
            Hide();
            
            _count++;
            string id = _count.ToString();
            _currentId = id;

            DoShow().Forget();

            return id;

            async UniTaskVoid DoShow()
            {
                _loadCancellationTokenSource?.Cancel();
                CancellationTokenSource cancellationTokenSource = new();
                _loadCancellationTokenSource = cancellationTokenSource;

                try {
                    IPopup popup = (IPopup) await _popupLoader.LoadPopupAsync(typeof(T), parameters, _popupContainer, cancellationTokenSource.Token);
                    if (_currentId != id) {
                        _popupLoader.Unload((MonoBehaviour) popup);
                        return;
                    }
                    _currentPopup = popup;
                    popup.Show(target, defaultAlign, offset).Forget();
                } catch (OperationCanceledException) {
                    // nothing to do
                } finally {
                    if (cancellationTokenSource == _loadCancellationTokenSource) {
                        _loadCancellationTokenSource = null;
                    }
                    cancellationTokenSource.Dispose();
                }
            }
        }

        public void Hide(string id)
        {
            if (_currentId != id) {
                return;
            }
            _currentId = null;
            IPopup? popup = _currentPopup;
            if (popup == null) {
                return;
            }

            MonoBehaviour popupBehaviour = (MonoBehaviour) popup;
            _hidingItems[id] = popup;

            DoHide().Forget();

            async UniTaskVoid DoHide()
            {
                try {
                    await popup.Hide();
                    await UniTask.Yield();
                } finally {
                    if (!popupBehaviour.IsDestroyed()) {
                        _popupLoader.Unload(popupBehaviour);
                    }
                    _hidingItems.Remove(id);
                }
            }
        }

        public void Hide()
        {
            if (_currentId == null) {
                return;
            }
            Hide(_currentId);
        }

        public bool IsShowing(string? id)
        {
            if (id == null) {
                return false;
            }
            return _currentId == id;
        }

        public bool IsHiding(string? id)
        {
            if (id == null) {
                return false;
            }
            return _hidingItems.ContainsKey(id);
        }
    }
}
