using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using UILabel = LiteUI.Element.Texts.UILabel;

namespace LiteUI.Element.Buttons
{
    [PublicAPI, RequireComponent(typeof(Animator))]
    public class AnimatedToggle : Toggle
    {
        private static readonly int ENABLE_ANIMATION_NAME = Animator.StringToHash("Enable");
        private static readonly int DISABLE_ANIMATION_NAME = Animator.StringToHash("Disable");
        private static readonly int ENABLED_ANIMATION_NAME = Animator.StringToHash("Enabled");

        private UILabel _label = null!;
        private Animator _animator = null!;

        private bool _ignoreClick = true;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (isOn) {
                _animator.SetTrigger(ENABLED_ANIMATION_NAME);
            }
        }

        public void Init(bool on)
        {
            _animator = GetComponent<Animator>();
            _label = GetComponentInChildren<UILabel>();

            onValueChanged.AddListener(OnToggleClick);

            isOn = on;
            if (on) {
                _animator.SetTrigger(ENABLED_ANIMATION_NAME);
            }

            _ignoreClick = false;
        }

        private void OnToggleClick(bool state)
        {
            if (_ignoreClick) {
                return;
            }
            IsOn = state;
        }

        public string Label
        {
            set
            {
                if (_label.IsDestroyed()) {
                    throw new NullReferenceException($"No label object to set text={value}");
                }
                _label.Text = value;
            }
        }

        public bool IsOn
        {
            get => isOn;
            set
            {
                SetIsOnWithoutNotify(value);
                _animator.SetTrigger(value ? ENABLE_ANIMATION_NAME : DISABLE_ANIMATION_NAME);
            }
        }

        internal bool IgnoreClicks => _ignoreClick;
    }
}
