using LiteUI.UI.Service;
using MessagePipe;
using Project.Core.Character;
using Project.Core.Core.Blockers;
using Project.Core.Core.State;
using Project.Core.Messages;
using Project.Core.Player;
using Project.Gameplay.Assets;
using Project.Gameplay.Character;
using Project.Gameplay.Config;
using Project.Gameplay.Gameplay.Player;
using Project.Gameplay.Gameplay.State;
using Project.Gameplay.Interaction;
using Project.Gameplay.Quest;
using Project.Gameplay.Save;
using Project.Gameplay.Spawn;
using Project.Unity.Bootstrap;
using Project.Unity.Config;
using Project.Unity.Player;
using Project.Unity.Quest;
using Project.Unity.Spawn;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Project.Unity.Installers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;
        
        [Header("Input")]
        [SerializeField] private WorldObjectCollector _worldObjectCollector;
        [SerializeField] private UiObjectCollector _uiObjectCollector;

        [Header("Configs")]
        [SerializeField] private GameConfigAsset _gameConfig;

        [Header("Quests")]
        [SerializeField] private QuestDatabase _questDatabase;

        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureMessagePipe(builder);
            ConfigureStates(builder);
            ConfigureCollectors(builder);
            ConfigureConfigs(builder);
            ConfigureInput(builder);
            builder.RegisterBuildCallback(InjectSceneComponents);
            ConfigurePlayer(builder);
            ConfigureServices(builder);
            ConfigureQuests(builder);
        }
        
        private void InjectSceneComponents(IObjectResolver resolver)
        {
            var spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (SpawnPoint? spawnPoint in spawnPoints)
            {
                resolver.Inject(spawnPoint);
            }
            var bootstraps = FindObjectsByType<MonoSceneBootstrap>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (MonoSceneBootstrap? bootstrap in bootstraps)
            {
                resolver.Inject(bootstrap);
            }
            resolver.Inject(_uiObjectCollector.RequireObjectByType<EyeController>());
        }

        private void ConfigureMessagePipe(IContainerBuilder builder)
        {
            MessagePipeOptions options = builder.RegisterMessagePipe();

            builder.RegisterMessageBroker<CharacterStateChangedMessage>(options);
            builder.RegisterMessageBroker<CharacterLandedMessage>(options);
            builder.RegisterMessageBroker<LadderInteractionMessage>(options);
            builder.RegisterMessageBroker<InteractionTargetChangedMessage>(options);
            builder.RegisterMessageBroker<InteractionPerformedMessage>(options);
            builder.RegisterMessageBroker<EyesStateChangedMessage>(options);
        }
        
        private void ConfigureStates(IContainerBuilder builder)
        {
            builder.Register<IGameStateService, GameStateService>(Lifetime.Singleton);
            builder.Register<IPlayerBlockerService, PlayerBlockerService>(Lifetime.Singleton);
            builder.Register<IPlayerControlState, PlayerControlState>(Lifetime.Singleton);
            builder.RegisterEntryPoint<EyesStateProcessor>();
        }

        private void ConfigureCollectors(IContainerBuilder builder)
        {
            if (_worldObjectCollector != null)
                builder.RegisterInstance(_worldObjectCollector);
            
            if (_uiObjectCollector != null)
                builder.RegisterInstance(_uiObjectCollector);
        }

        private void ConfigureConfigs(IContainerBuilder builder)
        {
            builder.Register<ConfigService>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<ConfigProvider>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterBuildCallback(resolver =>
            {
                if (_gameConfig == null)
                {
                    return;
                }
                ConfigService configService = resolver.Resolve<ConfigService>();
                configService.Register(_gameConfig);
                configService.Register(_gameConfig.CharacterMovement);
                configService.Register(_gameConfig.Interaction);
                configService.Register(_gameConfig.Camera);
                configService.Register(_gameConfig.UI);
            });
        }

        private void ConfigureInput(IContainerBuilder builder)
        {
            builder.RegisterInstance(_inputActions);
            builder.Register<MovementCommandDispatcher>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();
        }
        
        private void ConfigurePlayer(IContainerBuilder builder)
        {
            builder.Register<PlayerModel>(Lifetime.Scoped);

            builder.RegisterInstance(_gameConfig != null
                ? _gameConfig.CharacterMovement
                : new CharacterMovementSettings());
        }

        private void ConfigureServices(IContainerBuilder builder)
        {
            builder.Register<AssetService>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<SpawnService>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<SaveSystem>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<InteractionService>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();
        }

        private void ConfigureQuests(IContainerBuilder builder)
        {
            builder.Register<QuestService>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterBuildCallback(resolver =>
            {
                if (_questDatabase == null) return;

                QuestService questService = resolver.Resolve<QuestService>();
                foreach (QuestDataAsset questAsset in _questDatabase.Quests)
                {
                    if (questAsset != null)
                    {
                        questService.RegisterQuest(questAsset.ToQuestData());
                    }
                }
            });
        }
    }
}
