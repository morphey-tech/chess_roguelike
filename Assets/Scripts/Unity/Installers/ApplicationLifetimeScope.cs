using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Artifacts.Messages;
using Project.Gameplay.Gameplay.Assets;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Economy;
using Project.Gameplay.Gameplay.Logging;
using Project.Gameplay.Gameplay.Memory;
using Project.Gameplay.Gameplay.Save;
using Project.Gameplay.Gameplay.Save.Adapter;
using Project.Gameplay.Gameplay.Save.Service;
using Project.Gameplay.Gameplay.Scene;
using Project.Unity.Unity.Debug;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using IRandomService = Project.Core.Core.Random.IRandomService;
using RandomService = Project.Core.Core.Random.RandomService;
using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Artifacts;

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
            UnitySaveEnvironment saveEnvironment = new();

            // Register MessagePipe for Artifact messages
            MessagePipeOptions options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<ArtifactAddedMessage>(options);
            builder.RegisterMessageBroker<ArtifactRemovedMessage>(options);
            builder.RegisterMessageBroker<ArtifactsClearedMessage>(options);

            builder.Register<LogService>(Lifetime.Singleton)
                .WithParameter(_minLogLevel)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<RandomService>(Lifetime.Singleton)
                .As<IRandomService>()
                .AsSelf();

            builder.Register<AssetService>(Lifetime.Scoped)
                .AsImplementedInterfaces();
            builder.Register<ConfigProvider>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<ConfigHotReloadService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<TriggerService>(Lifetime.Singleton);
            builder.Register<PlayerLoadoutService>(Lifetime.Singleton);
            builder.Register<PlayerRunStateService>(Lifetime.Singleton);
            builder.Register<PlayerMetaProgressService>(Lifetime.Singleton);
            
            builder.Register<PlayerLoadoutSaveAdapter>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<PlayerRunStateSaveAdapter>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<PlayerMetaProgressSaveAdapter>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<PassiveFactory>(Lifetime.Singleton);
            builder.Register<ItemFactory>(Lifetime.Singleton);
            builder.Register<EconomyService>(Lifetime.Singleton)
                .AsSelf();
            builder.Register<EconomySaveAdapter>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            // Artifacts system (base services - accessible everywhere)
            builder.Register<ArtifactConfigRepository>(Lifetime.Singleton)
                .AsSelf();
            builder.Register<ArtifactFactory>(Lifetime.Singleton)
                .AsSelf();
            builder.Register<ArtifactService>(Lifetime.Singleton)
                .AsSelf();
            builder.Register<ArtifactSaveAdapter>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<MemoryCleanService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<SceneTransitionService>(Lifetime.Singleton);
            builder.Register<SceneService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            // Debug commands
            builder.Register<EconomyConsoleCommands>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<ArtifactConsoleCommands>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterInstance(saveEnvironment)
                .As<Project.Core.Core.Save.ISaveEnvironment>();
            builder.RegisterInstance(new FileSaveStorage(saveEnvironment.SavePath));
            builder.Register<SaveService>(Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}
