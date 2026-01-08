using System;
using LiteUI.Common.Logger;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UILabel = LiteUI.Element.Texts.UILabel;

namespace LiteUI.Element.Tab
{
    [PublicAPI]
    public class TabElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<TabElement>();
        
        [SerializeField]
        private GameObject? _content;
        [SerializeField]
        private GameObject? _label;
        [SerializeField]
        private Animator _animator = null!;
        [SerializeField]
        private Image? _pressedImage;
        [SerializeField]
        private Image? _iconImage;
        [SerializeField]
        private Color _normalColor = new(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField]
        private Color _pressedColor = Color.white;
        [SerializeField]
        private Color _highlightedColor = new(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField]
        private Color _disabledColor = new(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField]
        private Image? _backgroundImage;
        [SerializeField]
        private Sprite? _normalSprite;
        [SerializeField]
        private Sprite? _pressedSprite;
        [SerializeField]
        private Sprite? _highlightedSprite;
        [SerializeField]
        private Sprite? _disabledSprite;

        public event Action<TabElement>? OnTabClick;
        public event Action<TabElement, TabSelectionState>? OnSelectionChanged;
        

        private TabSelectionState _selectionState = TabSelectionState.NORMAL;
        private TabSelectionState _defferSelection = TabSelectionState.NORMAL;
        private bool _pointerOut;

        public TabGroup TabGroup { get; set; } = null!;
        public UnityEvent OnPointerUpEvent { get; } = new();

        private void OnEnable()
        {
            _selectionState = _defferSelection;
            _defferSelection = TabSelectionState.NORMAL;
            SetState(_selectionState);
        }

        private void OnDisable()
        {
            _defferSelection = _selectionState;
        }

        public void SetSelectionState(TabSelectionState state)
        {
            SetState(state);
        }

        public void TabClick()
        {
            if (_selectionState is TabSelectionState.DISABLED or TabSelectionState.PRESSED) {
                return;
            }
            OnTabClick?.Invoke(this);
            OnPointerUpEvent.Invoke();
        }

        public void OnPointerDown(PointerEventData? eventData)
        {
            if (_selectionState != TabSelectionState.NORMAL) {
                return;
            }
            SetSelectionState(TabSelectionState.HIGHLIGHTED);
        }

        public void OnPointerUp(PointerEventData? eventData)
        {
            if (eventData == null) {
                return;
            }
            if (!eventData.dragging) {
                if (!_pointerOut) {
                    TabClick();
                }
                return;
            }
            if (_selectionState == TabSelectionState.HIGHLIGHTED) {
                SetSelectionState(TabSelectionState.NORMAL);
            }
        }

        public void OnPointerEnter(PointerEventData? eventData)
        {
            _pointerOut = false;
            if (_selectionState != TabSelectionState.NORMAL) {
                return;
            }
            SetSelectionState(TabSelectionState.HIGHLIGHTED);
        }

        public void OnPointerExit(PointerEventData? eventData)
        {
            _pointerOut = true;
            if (_selectionState != TabSelectionState.HIGHLIGHTED) {
                return;
            }
            SetSelectionState(TabSelectionState.NORMAL);
        }
        
        public void SetContentActive(bool value)
        {
            if (_content == null) {
                return;
            }
            _content.gameObject.SetActive(value);
        }

        private void SetState(TabSelectionState state)
        {
            if (!IsValidAnimator() && _backgroundImage == null) {
                _defferSelection = state;
                return;
            }
            SelectionState = state;
        }

        private void SetAnimatorStateBool(TabSelectionState state, bool value)
        {
            if (!IsValidAnimator()) {
                return;
            }
            if (state == TabSelectionState.NORMAL) {
                return;
            }
            _animator.SetBool(state.GetName(), value);
        }

        private void SetBackgroundImageState(TabSelectionState state)
        {
            if (_backgroundImage == null) {
                return;
            }
            
            switch (state) {
                case TabSelectionState.NORMAL:
                    if (_normalSprite == null) {
                        _logger.Warn("Sprite for normal state is not assigned");
                        break;
                    }
                    _backgroundImage.sprite = _normalSprite;
                    break;
                case TabSelectionState.PRESSED:
                    if (_pressedSprite == null) {
                        _logger.Warn("Sprite for pressed state is not assigned");
                        break;
                    }
                    _backgroundImage.sprite = _pressedSprite;
                    break;
                case TabSelectionState.DISABLED:
                    if (_disabledSprite == null) {
                        _logger.Warn("Sprite for disabled state is not assigned");
                        break;
                    }
                    _backgroundImage.sprite = _disabledSprite;
                    break;
                case TabSelectionState.HIGHLIGHTED:
                    if (_highlightedSprite == null) {
                        _logger.Warn("Sprite for highlighted state is not assigned");
                        break;
                    }
                    _backgroundImage.sprite = _highlightedSprite;
                    break;
            }
        }
        
        private void SetIconImageState(TabSelectionState state)
        {
            if (_iconImage == null) {
                return;
            }
            
            switch (state) {
                case TabSelectionState.NORMAL:
                    _iconImage.color = _normalColor;
                    break;
                case TabSelectionState.PRESSED:
                    _iconImage.color = _pressedColor;
                    break;
                case TabSelectionState.DISABLED:
                    _iconImage.color = _disabledColor;
                    break;
                case TabSelectionState.HIGHLIGHTED:
                    _iconImage.color = _highlightedColor;
                    break;
            }
        }
        
        private void SetPressedImageState(TabSelectionState state)
        {
            if (_pressedImage == null) {
                return;
            }
            _pressedImage.gameObject.SetActive(state == TabSelectionState.PRESSED);
        }

        private bool IsValidAnimator()
        {
            return _animator != null && _animator.runtimeAnimatorController != null && _animator.gameObject.activeInHierarchy;
        }

        public string? Label
        {
            get
            {
                if (_label == null) {
                    _label = gameObject;
                }
                if (_label.GetComponentInChildren<UILabel>()) {
                    return _label.GetComponentInChildren<UILabel>().Text;
                }
                return _label.GetComponentInChildren<Text>() ? _label.GetComponentInChildren<Text>().text : null;
            }
            set
            {
                if (_label == null) {
                    _label = gameObject;
                }
                if (_label.GetComponentInChildren<UILabel>()) {
                    _label.GetComponentInChildren<UILabel>().Text = value ?? "";
                }
                if (_label.GetComponentInChildren<Text>()) {
                    _label.GetComponentInChildren<Text>().text = value ?? "";
                }
            }
        }

        public bool Interactable
        {
            set
            {
                if (!value) {
                    SetState(TabSelectionState.DISABLED);
                } else if (_selectionState == TabSelectionState.DISABLED) {
                    SetState(TabSelectionState.NORMAL);
                }
            }
        }

        public TabSelectionState SelectionState
        {
            get => _selectionState;
            private set
            {
                SetIconImageState(value);
                SetBackgroundImageState(value);
                SetPressedImageState(value);
                SetAnimatorStateBool(_selectionState, false);
                _selectionState = value;
                SetAnimatorStateBool(_selectionState, true);
                OnSelectionChanged?.Invoke(this, _selectionState);
            }
        }

        public GameObject? Content
        {
            get => _content;
            set => _content = value;
        }
    }
}
