using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Project.Core.Window
{
    [RequireComponent(typeof(Animator))]
    public class WindowAnimatorOpenController : MonoBehaviour
    {
        [SerializeField] private string _openBool = "opened";
        [SerializeField] private string _closedStateName = "closed";

        private bool _isTrigger;

        private Animator _animator;
        private CancellationTokenSource _cancellation;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            //if(!_animator.MMHasParameterOfType(_openBool, AnimatorControllerParameterType.Bool) && _animator.MMHasParameterOfType(_openBool, AnimatorControllerParameterType.Trigger))
                _isTrigger = true;
        }

        public bool CanBeClosed()
        {
            //todo remove this shit
            return _animator.GetCurrentAnimatorStateInfo(0).IsName(_closedStateName);
        }

        public void Show()
        {
            if (_isTrigger)
            {
                _animator.SetTrigger(_openBool);
            }
            else
            {
                _animator.SetBool(_openBool, true);
            }
        }

        public async UniTask HideAsync()
        {
            Hide();
            _cancellation.CancelAfterSlim(TimeSpan.FromSeconds(2));
            await UniTask.WaitUntil(CanBeClosed, cancellationToken: _cancellation.Token).SuppressCancellationThrow();
        }

        private void OnEnable()
        {
            _cancellation = new();
        }

        private void OnDisable()
        {
            _cancellation.Cancel();
            _cancellation?.Dispose();
        }

        public void Hide()
        {
            if(!_isTrigger)
                _animator.SetBool(_openBool, false);
        }
    }
}