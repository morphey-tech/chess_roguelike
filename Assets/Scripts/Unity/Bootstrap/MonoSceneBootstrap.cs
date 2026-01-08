using Cysharp.Threading.Tasks;
using Project.Core.Bootstrap;
using Project.Core.Logging;
using Project.Core.Scene;
using UnityEngine;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Unity.Bootstrap
{
    /// <summary>
    /// Базовый класс для бутстраперов сцен на основе MonoBehaviour.
    /// Просто добавь на сцену — SceneTransitionService найдёт и запустит автоматически.
    /// </summary>
    public abstract class MonoSceneBootstrap : MonoBehaviour, ISceneBootstrap
    {
        protected ILogger Log { get; private set; } = null!;
        protected SceneTransitionData? TransitionData { get; private set; }
        
        private IObjectResolver _resolver = null!;
        private bool _isBootstrapped;

        [Inject]
        public void Construct(IObjectResolver resolver, ILogService logService)
        {
            _resolver = resolver;
            Log = logService.CreateLogger(GetType().Name);
            OnConstruct();
        }

        /// <summary>
        /// Вызывается после инъекции зависимостей. 
        /// Переопределите для получения дополнительных зависимостей.
        /// </summary>
        protected virtual void OnConstruct() { }

        /// <summary>
        /// Запускает бутстрап сцены. Вызывается из SceneTransitionService.
        /// </summary>
        public async UniTask BootstrapAsync(SceneTransitionData? transitionData)
        {
            if (_isBootstrapped)
            {
                Log.Warning("Bootstrap already executed");
                return;
            }

            _isBootstrapped = true;
            TransitionData = transitionData;
            
            Log.Info("Bootstrap starting...");

            try
            {
                await OnBootstrapAsync();
                Log.Info("Bootstrap completed");
            }
            catch (System.Exception ex)
            {
                Log.Error("Bootstrap failed", ex);
                throw;
            }
        }

        /// <summary>
        /// Инъекция зависимостей в компонент через resolver.
        /// </summary>
        protected void Inject(object target)
        {
            _resolver.Inject(target);
        }

        /// <summary>
        /// Получить зависимость из контейнера.
        /// </summary>
        protected T Resolve<T>()
        {
            return _resolver.Resolve<T>();
        }

        /// <summary>
        /// Переопределите для добавления логики инициализации сцены.
        /// </summary>
        protected abstract UniTask OnBootstrapAsync();
    }
}
