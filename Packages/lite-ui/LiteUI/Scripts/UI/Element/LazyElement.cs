using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LiteUI.UI.Exceptions;
using LiteUI.UI.Model;
using LiteUI.UI.Service;
using UnityEngine;

namespace LiteUI.UI.Element
{
    public class LazyElement<T>
            where T : MonoBehaviour
    {
        private readonly UIService _uiService;
        private readonly UIModel _model;

        private CancellationTokenSource? _cancellationTokenSource;
        private UniTaskCompletionSource<T>? _completionSource;
            
        private T? _instance;

        public LazyElement(UIService uiService, UIModel model)
        {
            _uiService = uiService;
            _model = model;
        }

        public async UniTask<T> Load()
        {
            if (_instance != null) {
                return _instance;
            }
            if (_completionSource != null) {
                return await _completionSource.Task;
            }
            _completionSource = new UniTaskCompletionSource<T>();
            CreateInstance(_completionSource).SuppressCancellationThrow().Forget();
            return await _completionSource.Task;
        }

        public void Release()
        {
            _cancellationTokenSource?.Cancel();
                
            if (_instance == null) {
                return;
            }
            _uiService.Release(_instance.gameObject);
            _instance = null;
        }

        private async UniTask CreateInstance(UniTaskCompletionSource<T> completionSource)
        {
            try {
                _cancellationTokenSource = new CancellationTokenSource();
                _instance = await _uiService.CreateAsync<T>(_model, _cancellationTokenSource.Token);
                completionSource.TrySetResult(_instance);
            } catch (UICreateCanceledException e) {
                completionSource.TrySetException(new OperationCanceledException("UI Create cancelled", e));
            } finally {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }
}
