using System;
using LiteUI.Binding.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LiteUI.Binding.Components
{
    [UIController(nameof(UIClickComponent))]
    public class UIClickComponent : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
    {
        private const double LONG_CLICK_THRESHOLD = 0.2f;

        public event Action? OnTouch;
        public event Action? OnClick;
        public event Action? OnDoubleClick;
        public event Action? OnLongClick;
        private float _downTime;

        public bool InteractableTouch { get; set; } = true;
        public bool InteractableClick { get; set; } = true;
        public bool InteractableDoubleClick { get; set; } = true;
        public bool InteractableLongClick { get; set; } = true;

        private bool _useButtonState;
        private Button? _button;

        internal void Init(bool useButtonState)
        {
            _useButtonState = useButtonState;
            if (!useButtonState) {
                return;
            }
            _button = GetComponent<Button>();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount >= 2 && OnDoubleClick != null) {
                if (!InteractableDoubleClick) {
                    return;
                }
                OnDoubleClick.Invoke();
                return;
            }
            if (Time.unscaledTime - _downTime >= LONG_CLICK_THRESHOLD && OnLongClick != null) {
                if (!InteractableLongClick) {
                    return;
                }
                OnLongClick.Invoke();
                _downTime = 0;
                return;
            }
            _downTime = 0;

            if (!InteractableClick) {
                return;
            }

            if (_button == null) {
                OnClick?.Invoke();
                return;
            }
            
            if (ButtonActive) {
                OnClick?.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _downTime = Time.unscaledTime;
            if (!InteractableTouch) {
                return;
            }
            if (_button == null || ButtonActive) {
                OnTouch?.Invoke();
            }
        }

        private bool ButtonActive => _useButtonState && _button!.interactable && _button.isActiveAndEnabled;
    }
}
