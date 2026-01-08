using System;
using LiteUI.Common.Logger;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiteUI.Element.Widgets
{
    public class Joystick : MonoBehaviour, IJoystick, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<Joystick>();

        private const float HANDLE_RANGE = 1F;
        private const float DEAD_ZONE = 0.01F;

        public event Action<Vector2>? OnJoystick;

        [SerializeField]
        private RectTransform _background = null!;
        [SerializeField]
        private RectTransform _handle = null!;

        private Canvas? _canvas;

        private Vector2 _input = Vector2.zero;
        private bool _dispatchedStop = true;

        private void Start()
        {
            _canvas = GetComponentInParent<Canvas>();

            if (_canvas == null) {
                _logger.Error("The Joystick is not placed inside a canvas");
            }

            ApplyHandlerPosition();

            _background.gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            _input = Vector2.zero;
            ApplyHandlerPosition();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            EndMove();
        }

        public void EndMove()
        {
            _dispatchedStop = false;
            _input = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdatePosition(eventData.position);
        }

        public void UpdatePosition(Vector2 position)
        {
            if (_canvas == null) {
                return;
            }
            Camera? worldCamera = null;
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera) {
                worldCamera = _canvas.worldCamera;
            }

            Vector2 previousPosition = RectTransformUtility.WorldToScreenPoint(worldCamera, _background.position);
            Vector2 radius = _background.sizeDelta / 2;
            Vector2 currentInput = (position - previousPosition) / (radius * _canvas.scaleFactor);

            if (IsInZeroPosition(currentInput)) {
                _input = Vector2.zero;
            } else if (currentInput.magnitude > 1) {
                _input = currentInput.normalized;
            } else {
                _input = currentInput;
            }
            _handle.anchoredPosition = _input * radius * HANDLE_RANGE;
        }

        private void ApplyHandlerPosition()
        {
            Vector2 center = new(0.5f, 0.5f);
            _background.pivot = center;
            _handle.anchorMin = center;
            _handle.anchorMax = center;
            _handle.pivot = center;
            _handle.anchoredPosition = Vector2.zero;
        }

        private void FixedUpdate()
        {
            DispatchInputChanged();
        }

        private void DispatchInputChanged()
        {
            if (IsInZeroPosition(_input)) {
                if (_dispatchedStop) {
                    return;
                }
                _dispatchedStop = true;
            }
            OnJoystick?.Invoke(Direction);
        }

        private bool IsInZeroPosition(Vector2 input)
        {
            return input.magnitude < DEAD_ZONE;
        }

        public Vector3 Direction => new(_input.x, _input.y, 0);
    }
}
