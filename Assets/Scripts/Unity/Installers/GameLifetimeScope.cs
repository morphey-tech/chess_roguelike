using System;
using System.Collections.Generic;
using LiteUI.UI.Service;
using MessagePipe;
using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Messages;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Artifacts;
using Project.Gameplay.Gameplay.Board.Messages;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Interaction;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Flow;
using Project.Gameplay.Gameplay.Stage.Phase;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.Actions;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;
using Project.Gameplay.Gameplay.Installers;
using Project.Gameplay.Gameplay.Prepare.Messages;
using Project.Gameplay.Gameplay.Save.Adapter;
using Project.Gameplay.Gameplay.Shutdown;
using Project.Gameplay.Gameplay.Stage.Messages;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.UI;
using Project.Unity.Unity.Debug;
using Project.Unity.UI;
using Project.Gameplay.Presentations;
using Project.Gameplay.ShrinkingZone;
using Project.Gameplay.ShrinkingZone.Messages;
using Project.Unity.Unity.Bootstrap;
using Project.Unity.Unity.Prepare;
using Project.Unity.Unity.Views;
using Project.Unity.Unity.Views.Animations.Board;
using Project.Unity.Unity.Views.Animations.Board.Strategies;
using Project.Unity.Unity.World;
using Project.Unity.UI.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Project.Unity.Unity.Installers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;

        [Header("Collectors")]
        [SerializeField] private WorldObjectCollector _worldObjectCollector;
        [SerializeField] private UiObjectCollector _uiObjectCollector;

        private IObjectResolver _resolver = null!;
        private ILogger<GameLifetimeScope> _logger = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureMessagePipe(builder);
            ConfigureInput(builder);
            ConfigureCollectors(builder);
            ConfigureEntities(builder);
            ConfigureViews(builder);
            ConfigureServices(builder);
            builder.RegisterBuildCallback(OnContainerBuilt);
        }

        private void ConfigureEntities(IContainerBuilder builder)
        {
            builder.Register<EntityInstances>(Lifetime.Singleton)
                .AsSelf()
                .AsImplementedInterfaces();
            builder.Register<EntityService>(Lifetime.Singleton);
        }

        private void ConfigureMessagePipe(IContainerBuilder builder)
        {
            MessagePipeOptions options = builder.RegisterMessagePipe();
            options.HandlingSubscribeDisposedPolicy = HandlingSubscribeDisposedPolicy.Throw;
            options.EnableCaptureStackTrace = true;

            // Explicit registration of all message types for cross-scope usage
            // Input messages
            builder.RegisterMessageBroker<RawClickMessage>(options);
            builder.RegisterMessageBroker<CellClickedMessage>(options);
            builder.RegisterMessageBroker<EndTurnRequestedMessage>(options);
            builder.RegisterMessageBroker<CancelRequestedMessage>(options);
            builder.RegisterMessageBroker<FigureHoverChangedMessage>(options);
            builder.RegisterMessageBroker<RightClickMessage>(options);
            builder.RegisterMessageBroker<HandFigureClickedMessage>(options);

            // Turn messages
            builder.RegisterMessageBroker<TurnChangedMessage>(options);

            // Prepare messages
            builder.RegisterMessageBroker<PreparePhaseCompletedMessage>(options);
            builder.RegisterMessageBroker<PrepareSelectionChangedMessage>(options);
            builder.RegisterMessageBroker<PrepareVisualResetMessage>(options);
            builder.RegisterMessageBroker<PrepareCompleteRequestedMessage>(options);

            // Figure messages
            builder.RegisterMessageBroker<FigureSpawnedMessage>(options);
            builder.RegisterMessageBroker<FigureSelectedMessage>(options);
            builder.RegisterMessageBroker<FigureDeselectedMessage>(options);
            builder.RegisterMessageBroker<FigureDeathMessage>(options);
            builder.RegisterMessageBroker<FigureBoardRemovedMessage>(options);
            builder.RegisterMessageBroker<FigureAttackStartedMessage>(options);

            // Board messages
            builder.RegisterMessageBroker<BoardCapacityChangedMessage>(options);

            // Stage/Flow messages
            builder.RegisterMessageBroker<StageCompletedMessage>(options);
            builder.RegisterMessageBroker<PhaseStartedMessage>(options);
            builder.RegisterMessageBroker<PhaseCompletedMessage>(options);
            builder.RegisterMessageBroker<StageStartedMessage>(options);

            // Bonus move messages
            builder.RegisterMessageBroker<BonusMoveStartedMessage>(options);
            builder.RegisterMessageBroker<BonusMoveCompletedMessage>(options);

            // Storm messages
            builder.RegisterMessageBroker<StormBattleStartedMessage>(options);
            builder.RegisterMessageBroker<StormTurnStartedMessage>(options);
            builder.RegisterMessageBroker<StormStateChangedMessage>(options);
            builder.RegisterMessageBroker<StormCellsUpdatedMessage>(options);
            builder.RegisterMessageBroker<StormDamageDealtMessage>(options);
            builder.RegisterMessageBroker<StormFigureTurnEndedMessage>(options);
            builder.RegisterMessageBroker<FigureTakeStormDamageMessage>(options);

            // UI messages
            builder.RegisterMessageBroker<TooltipShowRequestMessage>(options);
            builder.RegisterMessageBroker<TooltipHideRequestMessage>(options);
        }

        private void ConfigureInput(IContainerBuilder builder)
        {
            builder.RegisterInstance(_inputActions);
        }

        private void ConfigureCollectors(IContainerBuilder builder)
        {
            if (_worldObjectCollector != null)
            {
                builder.RegisterInstance(_worldObjectCollector)
                    .AsSelf()
                    .As<IWorldRoot>();
            }
            if (_uiObjectCollector != null)
            {
                builder.RegisterInstance(_uiObjectCollector);
            }
        }

        private void ConfigureViews(IContainerBuilder builder)
        {
            // AnchorToTargetTicker — это ILateTickable, не MonoBehaviour
            // RegisterEntryPoint создаст экземпляр и будет вызывать LateTick()
            builder.RegisterEntryPoint<AnchorToTargetTicker>()
                .As<IAnchorToTargetTicker>();
            
            // Views are regular classes - they implement Core interfaces
            builder.Register<BoardPresenter>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            // Board appear
            builder.Register<BoardAnimationFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IBoardAnimationStrategy>>(new IBoardAnimationStrategy[]
                {
                    new NoneAnimationStrategy(),
                    new BoardCellsRainDropAnimationStrategy()
                });

            builder.Register<FigurePresenter>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ProjectilePresenter>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<LootPresenter>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ActionContextAccessor>(Lifetime.Singleton);

            // Visual command pipeline
            builder.Register<PresenterProvider>(Lifetime.Singleton)
                .As<IPresenterProvider>();
            builder.Register<VisualCommandExecutor>(Lifetime.Singleton);
            builder.Register<VisualPipeline>(Lifetime.Singleton);

            // Prepare zone — разделённые слои (provider, layout, factory, anim), presenter = оркестратор
            builder.Register<PrepareZoneAssetProvider>(Lifetime.Singleton)
                .As<IPrepareZoneAssetProvider>();
            builder.Register<PrepareLayoutService>(Lifetime.Singleton);
            builder.Register<PrepareViewFactory>(Lifetime.Singleton);
            builder.Register<PrepareAnimationPlayer>(Lifetime.Singleton);
            builder.Register<PreparePresenter>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            // Unity-side input handlers
            builder.Register<HandFigureClickHandler>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
        }

        private void ConfigureServices(IContainerBuilder builder)
        {
            // Gameplay (domain) scope — в модуле Gameplay
            GameplayContainerConfiguration.Register(builder);

            // Единая точка смерти и применения урона (зависимости от презентеров — здесь)
            builder.Register<FigureLifeService>(Lifetime.Singleton).As<IFigureLifeService>();
            builder.Register<DamageApplier>(Lifetime.Singleton);
            builder.Register<ProjectileHitApplyService>(Lifetime.Singleton).As<IProjectileHitApplyService>();

            // ActionBuilderContext (needs VisualPipeline, ActionContextAccessor, etc. registered above)
            builder.Register<ActionBuilderContext>(Lifetime.Singleton).As<IActionBuilderContext>();
            
            // Register SequentialActionBuilder after ActionBuilderRegistry is created
            builder.RegisterBuildCallback(resolver =>
            {
                var registry = resolver.Resolve<ActionBuilderRegistry>();
                registry.RegisterSequentialBuilder();
            });

            // Unity-only: консоль, UI, prepare-зона и фазы (LootPresenter — в ConfigureViews)
            builder.Register<ReloadStageConsoleCommands>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<StageOutcomeConsoleCommands>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<EconomyConsoleCommands>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<GameUiService>(Lifetime.Singleton).As<IGameUiService>();

            builder.Register<PrepareZonePrefabCache>(Lifetime.Singleton)
                .As<IPrepareZoneAssetPreloader>()
                .As<IPrepareZonePrefabCache>();
            builder.Register<PrepareZoneCachePhase>(Lifetime.Transient);
            builder.Register<PreparePlacementPhase>(Lifetime.Transient);
            builder.Register<PrepareHighlightService>(Lifetime.Singleton);
            builder.Register<PreparePlacementController>(Lifetime.Singleton);
            builder.Register<PrepareService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<PrepareInputHandler>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<PrepareVisualSyncService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<HpBarVisibilityService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<DamagePreviewService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<FigureInfoPreviewService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<UIAssetService>(Lifetime.Singleton).As<IUIAssetService>();
            builder.Register<TooltipService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }

        private void OnContainerBuilt(IObjectResolver resolver)
        {
            _resolver = resolver;
            _logger = resolver.Resolve<ILogService>()
                .CreateLogger<GameLifetimeScope>();

            // Force-create services with subscriptions
            resolver.Resolve<TurnService>();
            resolver.Resolve<InteractionController>();
            resolver.Resolve<ITurnController>();
            resolver.Resolve<IBonusMoveSession>(); // Has click subscription
            resolver.Resolve<StageService>();
            resolver.Resolve<RunFlowService>();
            resolver.Resolve<PrepareService>();
            resolver.Resolve<PrepareInputHandler>();
            resolver.Resolve<PrepareVisualSyncService>();
            resolver.Resolve<HpBarVisibilityService>();
            resolver.Resolve<DamagePreviewService>();
            resolver.Resolve<HandFigureClickHandler>();
            resolver.Resolve<StormBattleService>();
            resolver.Resolve<StormInitService>();
            resolver.Resolve<StormHighlightRenderer>();
            resolver.Resolve<StormDamageService>();
            resolver.Resolve<FigureInfoPreviewService>();

            // UI must be force-resolved so its constructor runs InitAsync
            // (loads WindowsController prefab). Without this, static UI methods
            // throw "UI is not valid" because _controller is never set.
            resolver.Resolve<Gameplay.Gameplay.UI.UIService>();

            // Inject MonoSceneBootstrap
            MonoSceneBootstrap? bootstrap = _worldObjectCollector.GetObjectByType<MonoSceneBootstrap>();
            if (bootstrap != null)
            {
                resolver.Inject(bootstrap);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                _resolver.Resolve<GameShutdownCleanupService>().Cleanup();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            base.OnDestroy();
        }
    }
}
