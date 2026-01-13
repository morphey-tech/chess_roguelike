using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Bootstrap;
using Project.Core.Core.Logging;
using Project.Core.Core.Scene;
using VContainer.Unity;
using ILogger = Project.Core.Core.Logging.ILogger;

namespace Project.Gameplay.Gameplay.Bootstrap
{
    /// <summary>
    /// Базовый класс для бутстраперов сцен через VContainer (не MonoBehaviour).
    /// Для MonoBehaviour-бутстраперов используйте MonoSceneBootstrap.
    /// </summary>
    public abstract class SceneBootstrapBase : ISceneBootstrap, IStartable, IDisposable
    {
        protected readonly ISceneService SceneService;
        protected readonly ILogger Logger;
        protected SceneTransitionData? TransitionData { get; private set; }
        
        public bool IsBootstrapping { get; private set; }
        public bool IsCompleted { get; private set; }
        
        private bool _disposed;
        
        protected SceneBootstrapBase(ISceneService sceneService, ILogService logService)
        {
            SceneService = sceneService;
            Logger = logService.CreateLogger(GetType().Name);
        }
        
        void IStartable.Start()
        {
            BootstrapAsync(null).Forget();
        }
        
        public async UniTask BootstrapAsync(SceneTransitionData? transitionData)
        {
            if (IsBootstrapping || IsCompleted)
            {
                Logger.Warning("Bootstrap already running or completed");
                return;
            }
            
            IsBootstrapping = true;
            TransitionData = transitionData;
            Logger.Info("Bootstrap starting...");
            
            try
            {
                await UniTask.DelayFrame(1);
                await OnBootstrapAsync();
                
                IsCompleted = true;
                Logger.Info("Bootstrap completed");
            }
            catch (Exception ex)
            {
                Logger.Error("Bootstrap failed", ex);
                throw;
            }
            finally
            {
                IsBootstrapping = false;
            }
        }
        
        /// <summary>
        /// Переопределите для добавления логики инициализации сцены
        /// </summary>
        protected abstract UniTask OnBootstrapAsync();
        
        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            Logger.Debug("Disposed");
        }
    }
}

