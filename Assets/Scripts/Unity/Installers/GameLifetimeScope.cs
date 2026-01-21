using System.Collections.Generic;
using LiteUI.UI.Service;
using MessagePipe;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Attack.Strategies;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Board.Appear;
using Project.Gameplay.Gameplay.Board.Appear.Strategies;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input;
using Project.Gameplay.Gameplay.Movement;
using Project.Gameplay.Gameplay.Movement.Strategies;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Phase;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Presentations;
using Project.Unity.Unity.Bootstrap;
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
            ConfigurePresentations(builder);
            ConfigureViews(builder);
            ConfigureServices(builder);
            builder.RegisterBuildCallback(OnContainerBuilt);
        }

        private void ConfigurePresentations(IContainerBuilder builder)
        {
            builder.Register<PresentationManager>(Lifetime.Singleton)
                .As<PresentationManager>();
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

            // Turn & Selection - subscribe to input events
            builder.Register<TurnSystem>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<SelectionService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            // Stage Service - handles stage events
            builder.Register<StageService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            // Prepare phase
            builder.Register<PrepareService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            // Figures spawn providers
            builder.Register<DuelFiguresSpawnProvider>(Lifetime.Singleton);
            builder.Register<EmptyFiguresSpawnProvider>(Lifetime.Singleton);
            builder.Register<IFiguresSpawnProviderFactory, FiguresSpawnProviderFactory>(Lifetime.Singleton);

            // Stage phases (Transient - created per stage)
            builder.Register<BoardSpawnPhase>(Lifetime.Transient);
            builder.Register<PreparePlacementPhase>(Lifetime.Transient);
            builder.Register<GameplayInitPhase>(Lifetime.Transient);
            builder.Register<BattleDuelPhase>(Lifetime.Transient);

            // Board - animation strategies
            builder.Register<BoardAppearAnimationFactory>(Lifetime.Singleton)
                .WithParameter(new List<IBoardAppearAnimationStrategy>
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
                    new RangedAttack()
                }); 

            // Combat resolver (passives come from figures)
            builder.Register<CombatResolver>(Lifetime.Singleton);

            // Pure gameplay services
            builder.Register<BoardSpawnService>(Lifetime.Singleton);
            builder.Register<FigureSpawnService>(Lifetime.Singleton);
            builder.Register<DamageService>(Lifetime.Singleton);
            builder.Register<MovementService>(Lifetime.Singleton);
        }

        private void OnContainerBuilt(IObjectResolver resolver)
        {
            // Принудительно создаём сервисы с подписками
            resolver.Resolve<TurnSystem>();
            resolver.Resolve<SelectionService>();
            resolver.Resolve<StageService>();
            resolver.Resolve<PrepareService>();
            resolver.Resolve<HandFigureClickHandler>();

            // Инжектим MonoSceneBootstrap
            MonoSceneBootstrap? bootstrap = _worldObjectCollector.GetObjectByType<MonoSceneBootstrap>();
            if (bootstrap != null)
            {
                resolver.Inject(bootstrap);
            }
        }
    }
}
