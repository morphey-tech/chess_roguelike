using Project.Core.Core.Logging;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Assets;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Logging;
using Project.Gameplay.Gameplay.Memory;
using Project.Gameplay.Gameplay.Save;
using Project.Gameplay.Gameplay.Save.Adapter;
using Project.Gameplay.Gameplay.Save.Service;
using Project.Gameplay.Gameplay.Scene;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Unity.Unity.Installers
{
    /// <summary>
    /// Корневой скоуп — синглтоны на всё время жизни игры.
    /// Размещается на DontDestroyOnLoad объекте.
    /// </summary>
    public class ApplicationLifetimeScope : LifetimeScope
    {
        [Header("Настройки логирования")]
        [SerializeField] private LogLevel _minLogLevel = LogLevel.Debug;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<LogService>(Lifetime.Singleton)
                .WithParameter(_minLogLevel)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<AssetService>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<ConfigProvider>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<SaveService>(Lifetime.Scoped)
                .AsImplementedInterfaces();
            builder.Register<PlayerLoadoutService>(Lifetime.Singleton);
            builder.Register<PlayerRunStateService>(Lifetime.Singleton);
            builder.Register<PlayerMetaProgressService>(Lifetime.Singleton);
            
            builder.Register<PlayerLoadoutSaveAdapter>(Lifetime.Singleton);
            builder.Register<PlayerRunStateSaveAdapter>(Lifetime.Singleton);
            builder.Register<PlayerMetaProgressSaveAdapter>(Lifetime.Singleton);
            
            
            builder.Register<MemoryCleanService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<SceneTransitionService>(Lifetime.Singleton);
            builder.Register<SceneService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}
