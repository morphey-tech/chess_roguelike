using System;
using Cysharp.Threading.Tasks;
using Project.Core.Logging;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Unity.Bootstrap
{
    public class SceneInitializer : MonoBehaviour
    {
        [Header("Инициализация")]
        [SerializeField] private InitializationStep[] _steps;

        [Header("События")]
        [SerializeField] private UnityEvent _onInitStarted;
        [SerializeField] private UnityEvent _onInitCompleted;
        [SerializeField] private UnityEvent<float> _onProgress;

        private ILogger _logger;
        private bool _initialized;

        public IObservable<float> OnProgressObservable => _onProgress.AsObservable().Select(e => e);

        [Inject]
        public void Construct(ILogService logService)
        {
            _logger = logService.CreateLogger<SceneInitializer>();
        }

        private async void Start()
        {
            await InitializeAsync();
        }

        public async UniTask InitializeAsync()
        {
            if (_initialized)
            {
                _logger?.Warning("Already initialized");
                return;
            }

            _logger?.Info("Scene initialization starting...");
            _onInitStarted?.Invoke();

            float totalWeight = 0f;
            foreach (InitializationStep step in _steps)
            {
                if (step.Enabled)
                {
                    totalWeight += step.Weight;
                }
            }

            float currentProgress = 0f;

            foreach (InitializationStep step in _steps)
            {
                if (!step.Enabled) continue;

                _logger?.Debug($"Executing step: {step.Name}");

                try
                {
                    if (step.DelayBefore > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(step.DelayBefore));
                    }

                    step.OnExecute?.Invoke();

                    if (step.DelayAfter > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(step.DelayAfter));
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error($"Step failed: {step.Name}", ex);

                    if (!step.ContinueOnError)
                    {
                        throw;
                    }
                }

                currentProgress += step.Weight / totalWeight;
                _onProgress?.Invoke(currentProgress);
            }

            _initialized = true;
            _onProgress?.Invoke(1f);
            _onInitCompleted?.Invoke();

            _logger?.Info("Scene initialization completed");
        }
    }

    [Serializable]
    public class InitializationStep
    {
        public string Name;
        public bool Enabled = true;
        public float Weight = 1f;
        public float DelayBefore = 0f;
        public float DelayAfter = 0f;
        public bool ContinueOnError = false;
        public UnityEvent OnExecute;
    }
}

