using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UILabel = LiteUI.Element.Texts.UILabel;

namespace LiteUI.Element.Widgets
{
    [PublicAPI]
    public class HorizontalSelect : UIBehaviour, IEventSystemHandler
    {
        [SerializeField]
        private Button _leftButton = null!;
        [SerializeField]
        private Button _rightButton = null!;
        [SerializeField]
        private UILabel _currentText = null!;
        [SerializeField]
        private List<string> _values = new();

        private string? _value;
        private bool _inited;
        
        public ChangeEvent OnChanged { get; set; } = new();


        protected override void Start()
        {
            _inited = true;

            _leftButton.onClick.AddListener(OnLeftClick);
            _rightButton.onClick.AddListener(OnRightClick);

            InvalidateValue();
        }

        protected override void OnDestroy()
        {
            _leftButton.onClick.RemoveListener(OnLeftClick);
            _rightButton.onClick.RemoveListener(OnRightClick);
        }

        private void OnRightClick()
        {
            if (Index + 1 >= _values.Count) {
                return;
            }

            Index++;
        }

        private void OnLeftClick()
        {
            if (Index - 1 < 0) {
                return;
            }

            Index--;
        }

        private void InvalidateValue()
        {
            if (_value == null && _values.Count > 0) {
                _value = _values[0];
            }

            if (_values.Count <= 0) {
                return;
            }
            UpdateCurrentValueText();
            UpdateControls();
        }

        private void SetCurrentValue(string? value)
        {
            if (value == _value) {
                return;
            }

            if (!_values.Contains(value ?? "")) {
                throw new ArgumentException($"Set current value error. Attempt to set invalid value={value}");
            }

            _value = value;

            UpdateCurrentValueText();
            UpdateControls();

            OnChanged.Invoke();
        }

        private void UpdateCurrentValueText()
        {
            if (_inited) {
                _currentText.Text = _value ?? "";
            }
        }

        private void SetValues(List<string> values)
        {
            if (values.Count == 0) {
                throw new ArgumentException("No values to set.");
            }

            _values = values;

            if (!_values.Contains(_value ?? "")) {
                Value = _values[0];
            } else {
                UpdateControls();
            }
        }

        private void SetCurrentIndex(int value)
        {
            if (value < 0 || value >= _values.Count) {
                throw new ArgumentException($"Index out of bounds. index={value}");
            }

            int index = _values.IndexOf(_value ?? "");
            if (value == index) {
                return;
            }

            SetCurrentValue(_values[value]);
        }

        private void UpdateControls()
        {
            if (!_inited) {
                return;
            }

            int index = Index;

            _leftButton.interactable = index > 0;
            _rightButton.interactable = index < _values.Count - 1;
        }

        public string? Value
        {
            get => _value;
            set => SetCurrentValue(value);
        }

        public List<string> Values
        {
            get => _values;
            set => SetValues(value);
        }

        public int Index
        {
            get => _values.IndexOf(_value ?? "");
            set => SetCurrentIndex(value);
        }

        [Serializable]
        public class ChangeEvent : UnityEvent
        {
        }
    }
}
