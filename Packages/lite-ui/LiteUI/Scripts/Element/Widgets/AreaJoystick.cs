using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiteUI.Element.Widgets
{
    public class AreaJoystick : MonoBehaviour, IJoystick, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const float DEAD_ZONE = 0.01F;
        private const int TRAIL_THRESHOLD = 50;
        private const int TRAIL_OFFSET = 20;

        public event Action<Vector2>? OnJoystick;

        [SerializeField]
        private RectTransform _handle = null!;
        [SerializeField]
        private RectTransform _trail = null!;
        [SerializeField]
        private Vector2 _radius = Vector2.one;
        [SerializeField]
        private float _joystickRange = 2f;
        private Vector2 _startPosition = Vector2.zero;
        private Vector2 _input = Vector2.zero;
        private bool _dispatchedStop = true;
        private Canvas _canvas = null!;
        private float _trailTreshold;

        private void Start()
        {
            _handle.gameObject.SetActive(false);
            _canvas = GetComponentInParent<Canvas>();

            Vector2 center = new(0.5f, 0.5f);
            gameObject.GetComponent<RectTransform>().pivot = center;
            _handle.anchorMin = center;
            _handle.anchorMax = center;
            _handle.pivot = center;
            _handle.anchoredPosition = Vector2.zero;
            _trailTreshold = _trail.sizeDelta.y + TRAIL_THRESHOLD;
            _trail.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            DispatchInputChanged();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _startPosition = eventData.pressPosition;
            _handle.gameObject.SetActive(true);
            _handle.position = _startPosition;
            _trail.position = _startPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 currentInput = (eventData.position - _startPosition) / (_radius / _joystickRange * _canvas.scaleFactor);

            if (IsInZeroPosition(currentInput)) {
                _input = Vector2.zero;
            } else if (currentInput.magnitude > 1) {
                _input = currentInput.normalized;
            } else {
                _input = currentInput;
            }
            Vector2 newPosition = _startPosition + _input * _radius;
            float length = (newPosition - _startPosition).magnitude - _handle.sizeDelta.x / 2 - TRAIL_OFFSET;
            _trail.gameObject.SetActive(length > _trailTreshold);
            _trail.sizeDelta = new Vector2(length, _trail.sizeDelta.y);

            Vector3 dir = newPosition - _startPosition;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = _trail.rotation;
            _trail.rotation = Quaternion.Euler(rotation.x, rotation.y, angle);
            _handle.position = newPosition;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _startPosition = Vector2.zero;
            _input = Vector2.zero;
            _handle.gameObject.SetActive(false);
            _trail.gameObject.SetActive(false);
            _dispatchedStop = false;
        }

        private void DispatchInputChanged()
        {
            if (IsInZeroPosition(_input)) {
                if (_dispatchedStop) {
                    return;
                }
                _dispatchedStop = true;
            }
            OnJoystick?.Invoke(_input);
        }

        private bool IsInZeroPosition(Vector2 input)
        {
            return input.magnitude < DEAD_ZONE;
        }
    }
}
