using System;
using System.Collections.Generic;
using LiteUI.UI.Service;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
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
using Project.Gameplay.Gameplay.Shutdown;
using Project.Gameplay.UI;
using Project.Unity.Unity.Debug;
using Project.Unity.UI;
using Project.Gameplay.Presentations;
using Project.Gameplay.ShrinkingZone;
using Project.Unity.Unity.Bootstrap;
using Project.Unity.Unity.Prepare;
using Project.Unity.Unity.Views;
using Project.Unity.Unity.Views.Animations.Board;
using Project.Unity.Unity.Views.Animations.Board.Strategies;
using Project.Unity.Unity.World;
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
            // Views are regular classes - they implement Core interfaces
            builder.Register<BoardPresenter>(Lifetime.Singleton)
                .As<IBoardPresenter>()
                .As<IGameShutdownCleanup>();

            // Board appear
            builder.Register<BoardAnimationFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IBoardAnimationStrategy>>(new IBoardAnimationStrategy[]
                {
                    new NoneAnimationStrategy(),
                    new BoardCellsRainDropAnimationStrategy()
                });
            
            builder.Register<FigurePresenter>(Lifetime.Singleton)
                .As<IFigurePresenter>()
                .As<IGameShutdownCleanup>();

            builder.Register<ProjectilePresenter>(Lifetime.Singleton)
                .As<IProjectilePresenter>()
                .As<IGameShutdownCleanup>();

            builder.Register<LootPresenter>(Lifetime.Singleton)
                .As<ILootPresenter>()
                .As<IGameShutdownCleanup>();

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
                .As<IPreparePresenter>()
                .As<IGameShutdownCleanup>();

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
            resolver.Resolve<ZoneBattleService>();
            resolver.Resolve<ZoneInitService>();
            resolver.Resolve<ZoneHighlightRenderer>();
            resolver.Resolve<ZoneDamageService>();

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
