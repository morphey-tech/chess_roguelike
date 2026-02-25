

using System;
using DG.Tweening;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Unity.UI.Components.Game;
using TMPro;
using UnityEngine;

namespace Project.Unity.UI.Components
{
    [RequireComponent(typeof(AnchorToTarget))]
    public class DamageText : MonoBehaviour, ICompletable
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _duration;
        [SerializeField] private AnimationCurve _moveCurve;
        [SerializeField] private AnimationCurve _scaleCurve;

        private Action<ICompletable> _onComplete;
        private AnchorToTarget _anchorToTarget;
        private Animator _animator;
        private CanvasGroup _canvasGroup;

        Component ICompletable.Value => this;

        private void Awake()
        {
            _anchorToTarget = GetComponent<AnchorToTarget>();
            _animator = GetComponent<Animator>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_animator != null)
            {
                _animator.enabled = false;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        void ICompletable.SetOnCompleteAction(Action<ICompletable> action)
        {
            _onComplete = action;
        }

        public void Play(DamageVisualContext ctx)
        {
            if (ctx.IsDodged)
            {
                _text.text = "MISS";
            }
            else
            {
                _text.text = ctx.Amount.ToString();
            }

            if (_animator != null)
            {
                _animator.enabled = true;
                _animator.Play(0, 0, 0f);
                _animator.Update(0f);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        public void OnAnimationEnd()
        {
            _onComplete?.Invoke(this);
        }
    }
}