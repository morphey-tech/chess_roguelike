using System;
using LiteUI.Binding.Attributes;
using LiteUI.Binding.Extension;
using LiteUI.Common.Utils;
using LiteUI.UI.Event;
using LiteUI.UI.Model;
using LiteUI.UI.Service;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace LiteUI.UI.Controller
{
    public class SafeAreaController : MonoBehaviour
    {
        [Binded]
        private RectTransform _safeAreaRectTransform = null!;
        
        private ScreenLayout? _screenLayout;
        private IPublisher<string, SafeAreaEvent> _safeAreaPublisher = null!;
        
        private Rect _lastSafeArea = Rect.zero;
        private Vector2Int _lastScreenSize = Vector2Int.zero;
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;
        private SafeAreaInsets _lastSafeAreaInsets = new(0, 0, 0, 0);

        private IDisposable? _disposable;

        [Inject]
        private void Construct(ScreenLayout screenLayout, IPublisher<string, SafeAreaEvent> safeAreaPublisher, ISubscriber<string, SafeAreaEvent> safeAreaSubscriber)
        {
            _screenLayout = screenLayout;
            _safeAreaPublisher = safeAreaPublisher;

            DisposableBagBuilder d = DisposableBag.CreateBuilder();
            safeAreaSubscriber.Subscribe(SafeAreaEvent.BANNER_AD_CHANGED, OnBannerAdChanged).AddTo(d);
            _disposable = d.Build();
        }
        
        private void Awake()
        {
            this.BindComponents();
        }
        
        private void Update()
        {
            if (_screenLayout == null) {
                return;
            }
            if (!IsScreenParamsChanged) {
                return;
            }
            ApplySafeArea();
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
            _disposable = null!;
        }

        private void ApplySafeArea()
        {
            if (Screen.width <= 0 || Screen.height <= 0) {
                return;
            }
            
            _lastOrientation = Screen.orientation;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            _lastSafeArea = _screenLayout!.SafeArea;
            
            Vector2 anchorMin = _lastSafeArea.position;
            Vector2 anchorMax = _lastSafeArea.position + _lastSafeArea.size;
            anchorMin.x = (anchorMin.x / Screen.width) + AnchorMinXInsetDelta;
            anchorMin.y = (anchorMin.y / Screen.height) + AnchorMinYInsetDelta;
            anchorMax.x = (anchorMax.x / Screen.width) - AnchorMaxXInsetDelta;
            anchorMax.y = (anchorMax.y / Screen.height) - AnchorMaxYInsetDelta;
            if (anchorMin.x < 0 || anchorMin.y < 0 || anchorMax.x < 0 || anchorMax.y < 0) {
                return;
            }
            _safeAreaRectTransform.anchorMin = anchorMin;
            _safeAreaRectTransform.anchorMax = anchorMax;
            _safeAreaPublisher.Publish(SafeAreaEvent.CHANGED, new SafeAreaEvent());
        }   

        private void OnBannerAdChanged(SafeAreaEvent evt)
        {
            SafeAreaInsets? insets = evt.Insets;
            if (insets == null) {
                return;
            }
            _lastSafeAreaInsets = insets;
            ApplySafeArea();
        }

        private bool IsScreenParamsChanged => _screenLayout!.SafeArea != _lastSafeArea || Screen.orientation != _lastOrientation 
                                              || !MathUtils.IsFloatEquals(_lastScreenSize.x, Screen.width)
                                              || !MathUtils.IsFloatEquals(_lastScreenSize.y, Screen.height);
        private float AnchorMinXInsetDelta => _lastSafeAreaInsets.Left == 0 ? 0 : (float) _lastSafeAreaInsets.Left / Screen.width;
        private float AnchorMinYInsetDelta => _lastSafeAreaInsets.Bottom == 0 ? 0 : (float) _lastSafeAreaInsets.Bottom / Screen.height;
        private float AnchorMaxXInsetDelta => _lastSafeAreaInsets.Right == 0 ? 0 : (float) _lastSafeAreaInsets.Right / Screen.width;
        private float AnchorMaxYInsetDelta => _lastSafeAreaInsets.Top == 0 ? 0 : (float) _lastSafeAreaInsets.Top / Screen.height;
    }
}
