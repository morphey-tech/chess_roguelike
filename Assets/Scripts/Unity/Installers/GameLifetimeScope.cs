using System.Collections.Generic;
using LiteUI.UI.Service;
using MessagePipe;
using Project.Core.Core.World;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Attack.Strategies;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Board.Appear;
using Project.Gameplay.Gameplay.Board.Appear.Strategies;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input;
using Project.Gameplay.Gameplay.Interaction;
using Project.Gameplay.Gameplay.Movement;
using Project.Gameplay.Gameplay.Movement.Strategies;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Phase;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using Project.Gameplay.Gameplay.Turn.Conditions;
using Project.Gameplay.Gameplay.Turn.Conditions.Impl;
using Project.Gameplay.Gameplay.Turn.Execution;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;
using Project.Gameplay.Presentations;
using Project.Unity.Unity.Bootstrap;
using Project.Unity.Unity.Prepare;
using Project.Unity.Unity.Views;
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
                .As<IBoardPresenter>();

            builder.Register<FigurePresenter>(Lifetime.Singleton)
                .As<IFigurePresenter>();

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
                .As<IPreparePresenter>();

            // Unity-side input handlers
            builder.Register<HandFigureClickHandler>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
        }

        private void ConfigureServices(IContainerBuilder builder)
        {
            // Run & Stage
            builder.Register<RunHolder>(Lifetime.Singleton);
            builder.Register<RunFactory>(Lifetime.Singleton);
            builder.Register<StageFactory>(Lifetime.Singleton);
            builder.Register<StagePhaseFactory>(Lifetime.Singleton);

            // Input - dispatches input events via MessagePipe
            builder.Register<InputDispatcher>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            // Turn system
            builder.Register<TurnSystem>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            // Interaction layer
            builder.Register<InteractionLockService>(Lifetime.Singleton)
                .As<IInteractionLock>();
            builder.Register<ClickIntentResolver>(Lifetime.Singleton)
                .As<IClickIntentResolver>();
            builder.Register<InteractionController>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<TurnController>(Lifetime.Singleton)
                .As<ITurnController>();

            // Stage Service - handles stage events
            builder.Register<StageService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            // Prepare phase — один кэш: заполняется во время доски, отдаёт префабы без задержки
            builder.Register<PrepareZonePrefabCache>(Lifetime.Singleton)
                .As<IPrepareZoneAssetPreloader>()
                .As<IPrepareZonePrefabCache>();
            builder.Register<PrepareService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            // Figures spawn providers
            builder.Register<SpawnPatternParser>(Lifetime.Singleton);
            builder.Register<DuelFiguresSpawnProvider>(Lifetime.Singleton);
            builder.Register<EmptyFiguresSpawnProvider>(Lifetime.Singleton);
            builder.Register<IFiguresSpawnProviderFactory, FiguresSpawnProviderFactory>(Lifetime.Singleton);

            // Stage phases (Transient - created per stage)
            builder.Register<PrepareZoneCachePhase>(Lifetime.Transient);
            builder.Register<BoardSpawnPhase>(Lifetime.Transient);
            builder.Register<PreparePlacementPhase>(Lifetime.Transient);
            builder.Register<GameplayInitPhase>(Lifetime.Transient);
            builder.Register<BattleDuelPhase>(Lifetime.Transient);

            // Board - animation strategies
            builder.Register<BoardAppearAnimationFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IBoardAppearAnimationStrategy>>(new IBoardAppearAnimationStrategy[]
                {
                    new BoardNoneAppearStrategy(),
                    new BoardWaveAppearStrategy()
                });

            // Movement strategies
            builder.Register<MovementStrategyFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IMovementStrategy>>(new IMovementStrategy[]
                {
                    new PawnMovement(),
                    new KnightMovement(),
                    new RookMovement(),
                    new BishopMovement(),
                    new QueenMovement()
                });

            // Attack strategies
            builder.Register<AttackStrategyFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IAttackStrategy>>(new IAttackStrategy[]
                {
                    new SimpleAttack(),
                    new RangedAttack(),
                    new SplashAttack(),
                    new PierceAttack()
                }); 

            // Combat system
            builder.Register<PassiveTriggerService>(Lifetime.Singleton);
            builder.Register<CombatResolver>(Lifetime.Singleton);

            // Turn pattern system
            builder.Register<ConditionRegistry>(Lifetime.Singleton)
                .WithParameter<IEnumerable<ITurnCondition>>(new ITurnCondition[]
                {
                    new AlwaysTrueCondition(),
                    new EnemyInRangeCondition(),
                    new EnemyAdjacentCondition(),
                    new HasTargetCondition(),
                    new TargetIsEnemyCondition(),
                    new TargetIsEmptyCondition(),
                    new CanMoveCondition()
                });
            builder.Register<TurnStepFactory>(Lifetime.Singleton);
            builder.Register<TurnPatternFactory>(Lifetime.Singleton);
            builder.Register<TurnPatternResolver>(Lifetime.Singleton);
            
            // Turn execution
            builder.Register<TurnExecutor>(Lifetime.Singleton)
                .As<ITurnExecutor>();
            builder.Register<BonusMoveController>(Lifetime.Singleton)
                .As<IBonusMoveController>();
            builder.Register<BonusMoveSession>(Lifetime.Singleton)
                .As<IBonusMoveSession>();

            // Pure gameplay services
            builder.Register<BoardSpawnService>(Lifetime.Singleton);
            builder.Register<FigureSpawnService>(Lifetime.Singleton);
            builder.Register<DamageService>(Lifetime.Singleton);
            builder.Register<MovementService>(Lifetime.Singleton);
            builder.Register<UI>(Lifetime.Singleton);
        }

        private void OnContainerBuilt(IObjectResolver resolver)
        {
            // Force-create services with subscriptions
            resolver.Resolve<TurnSystem>();
            resolver.Resolve<InteractionController>();
            resolver.Resolve<ITurnController>();
            resolver.Resolve<IBonusMoveSession>(); // Has click subscription
            resolver.Resolve<StageService>();
            resolver.Resolve<PrepareService>();
            resolver.Resolve<HandFigureClickHandler>();

            // Inject MonoSceneBootstrap
            MonoSceneBootstrap? bootstrap = _worldObjectCollector.GetObjectByType<MonoSceneBootstrap>();
            if (bootstrap != null)
            {
                resolver.Inject(bootstrap);
            }
        }
    }
}
