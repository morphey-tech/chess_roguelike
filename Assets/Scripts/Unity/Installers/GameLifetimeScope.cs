using System;
using System.Collections.Generic;
using LiteUI.Addressable.Service;
using LiteUI.Binding;
using LiteUI.UI.Registry;
using LiteUI.UI.Service;
using MessagePipe;
using Project.Core.Core.Annotation;
using Project.Core.Core.Configs.Artifacts;
using Project.Core.Core.Filters;
using Project.Core.Core.Filters.Messages;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Messages;
using Project.Core.Core.Triggers;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Artifacts;
using Project.Gameplay.Gameplay.Artifacts.Messages;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Board.Capacity;
using Project.Gameplay.Gameplay.Board.Messages;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Economy;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Filters;
using Project.Gameplay.Gameplay.Filters.Impl;
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
using UIService = Project.Gameplay.Gameplay.UI.UIService;

namespace Project.Unity.Unity.Installers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;

        [Header("Collectors")]
        [SerializeField] private WorldObjectCollector _worldObjectCollector;
        [SerializeField] private UiObjectCollector _uiObjectCollector;

        private readonly IObjectResolver _resolver = null!;
        private ILogger<GameLifetimeScope> _logger = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureMessagePipe(builder);
            ConfigureInput(builder);
            ConfigureCollectors(builder);
            ConfigureEntities(builder);
            ConfigureViews(builder);
            ConfigureServices(builder);
            Construct(builder);
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

            // Input messages
            builder.RegisterMessageBroker<RawClickMessage>(options);
            builder.RegisterMessageBroker<CellClickedMessage>(options);
            builder.RegisterMessageBroker<HandFigureClickedMessage>(options);
            builder.RegisterMessageBroker<EndTurnRequestedMessage>(options);
            builder.RegisterMessageBroker<CancelRequestedMessage>(options);
            builder.RegisterMessageBroker<FigureHoverChangedMessage>(options);
            builder.RegisterMessageBroker<RightClickMessage>(options);

            // Turn messages
            builder.RegisterMessageBroker<TurnChangedMessage>(options);

            // Prepare messages
            builder.RegisterMessageBroker<string, PrepareMessage>(options);

            // Figure messages
            builder.RegisterMessageBroker<string, FigureSelectMessage>(options);
            builder.RegisterMessageBroker<string, FigureBoardMessage>(options);
            builder.RegisterMessageBroker<FigureAttackMessage>(options);
            builder.RegisterMessageBroker<FigureDiedMessage>(options);

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
            builder.RegisterMessageBroker<string, TooltipMessage>(options);

            builder.RegisterMessageBroker<ArtifactAddedMessage>(options);
            builder.RegisterMessageBroker<ArtifactRemovedMessage>(options);
            builder.RegisterMessageBroker<ArtifactsClearedMessage>(options);
            builder.RegisterMessageBroker<string, AppFilterMessage>(options);
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
            builder.Register<FigurePresenter>(Lifetime.Scoped)
                .AsImplementedInterfaces();
            builder.Register<BoardPresenter>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            // Board appear
            builder.Register<BoardAnimationFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IBoardAnimationStrategy>>(new IBoardAnimationStrategy[]
                {
                    new NoneAnimationStrategy(),
                    new BoardCellsRainDropAnimationStrategy()
                });

            builder.RegisterEntryPoint<FigurePresenter>();

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

            builder.RegisterEntryPoint<HandFigureClickHandler>();
        }

        private void ConfigureServices(IContainerBuilder builder)
        {
            //LiteUI
            builder.Register<UIMetaRegistry>(Lifetime.Singleton);
            builder.Register<BindingService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BindingMonoBehaviourService>();
            builder.Register<AddressableManager>(Lifetime.Singleton);

            // UI Services
            builder.RegisterEntryPoint<AnchorToTargetTicker>().As<IAnchorToTargetTicker>();
            builder.Register<UIAssetService>(Lifetime.Singleton).As<IUIAssetService>();
            builder.Register<WindowsControllerInitializer>(Lifetime.Singleton);
            builder.Register<UIService>(Lifetime.Singleton).As<IUIService>();
            
            //Capacity
            builder.Register<BoardCapacityModel>(Lifetime.Singleton);
            builder.Register<BoardCapacityService>(Lifetime.Singleton);
            
            //TODO: скорее всего придется разбивать фильтры, на app и game хз пока
            builder.Register<AnnotationScanService>(Lifetime.Singleton);
            builder.Register<AppFilterService>(Lifetime.Transient)
                .As<IAppFilterService>();
            builder.Register<AddressablesInitFilter>(Lifetime.Transient);
            builder.Register<AnnotationScanFilter>(Lifetime.Transient);
            builder.Register<UIInitializationFilter>(Lifetime.Transient);
            
            builder.Register<TriggerService>(Lifetime.Singleton);
            builder.Register<ItemFactory>(Lifetime.Singleton);
            builder.Register<EconomyService>(Lifetime.Singleton)
                .AsSelf();
            builder.Register<EconomySaveAdapter>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ArtifactConfigRepository>(Lifetime.Singleton);
            builder.Register<ArtifactFactory>(Lifetime.Singleton);
            builder.Register<ArtifactService>(Lifetime.Singleton);
            builder.Register<ArtifactSaveAdapter>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            builder.Register<ArtifactSynergyRegistry>(Lifetime.Singleton);
            
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
                ActionBuilderRegistry registry = resolver.Resolve<ActionBuilderRegistry>();
                registry.RegisterSequentialBuilder();
            });

            // Unity-only: консоль, UI, prepare-зона и фазы (LootPresenter — в ConfigureViews)
            builder.Register<GameUiService>(Lifetime.Singleton).As<IGameUiService>();

            builder.Register<PrepareZonePrefabCache>(Lifetime.Singleton)
                .As<IPrepareZoneAssetPreloader>()
                .As<IPrepareZonePrefabCache>();
            builder.Register<PrepareZoneCachePhase>(Lifetime.Transient);
            builder.Register<PreparePlacementPhase>(Lifetime.Transient);
            builder.Register<PrepareHighlightService>(Lifetime.Singleton);
            builder.Register<PreparePlacementController>(Lifetime.Singleton);
            builder.Register<PrepareService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.RegisterEntryPoint<PrepareInputHandler>();
            builder.RegisterEntryPoint<PrepareVisualSyncService>();
            builder.RegisterEntryPoint<HpBarVisibilityService>();
            builder.RegisterEntryPoint<DamagePreviewService>();
            builder.RegisterEntryPoint<FigureInfoPreviewService>();
            builder.RegisterEntryPoint<TooltipService>();
            
            // Debug commands
            builder.RegisterEntryPoint<EconomyConsoleCommands>();
            builder.RegisterEntryPoint<ArtifactConsoleCommands>();
            builder.RegisterEntryPoint<ReloadStageConsoleCommands>();
            builder.RegisterEntryPoint<StageOutcomeConsoleCommands>();
        }
        
        private void Construct(IContainerBuilder builder)
        {
            builder.RegisterBuildCallback(r =>
            {
                r.Resolve<IFigurePresenter>();
                r.Resolve<IBoardPresenter>();
                r.Resolve<IStageHighlightRenderer>();
            });
        }

        protected override void OnDestroy()
        {
            try
            {
                _resolver.Resolve<GameShutdownCleanupService>().Cleanup();
            }
            catch (Exception ex)
            {
                _logger = _resolver.Resolve<ILogger<GameLifetimeScope>>();
                _logger.Error(ex.Message);
            }
            base.OnDestroy();
        }
    }
}
