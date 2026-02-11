using System.Collections.Generic;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Attack.Strategies;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Board.Appear;
using Project.Gameplay.Gameplay.Board.Appear.Strategies;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input;
using Project.Gameplay.Gameplay.Interaction;
using Project.Gameplay.Gameplay.Loot;
using Project.Gameplay.Gameplay.Movement;
using Project.Gameplay.Gameplay.Movement.Strategies;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Shutdown;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Phase;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using Project.Gameplay.Gameplay.Turn.Conditions;
using Project.Gameplay.Gameplay.Turn.Conditions.Impl;
using Project.Gameplay.Gameplay.Turn.Execution;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;
using VContainer;

namespace Project.Gameplay.Gameplay.Installers
{
    /// <summary>
    /// Registers all gameplay (domain) services. No Unity view types — call from Unity scope and add presenters there.
    /// ILootPresenter is registered as ApplyOnlyLootPresenter; Unity can override with LootPresenter.
    /// </summary>
    public static class GameplayContainerConfiguration
    {
        public static void Register(IContainerBuilder builder)
        {
            // Run & Stage
            builder.Register<RunHolder>(Lifetime.Singleton);
            builder.Register<RunFactory>(Lifetime.Singleton);
            builder.Register<StageFactory>(Lifetime.Singleton);
            builder.Register<StagePhaseFactory>(Lifetime.Singleton);

            // Loot (ILootPresenter регистрируется в Unity в ConfigureViews вместе с другими презентерами)
            builder.Register<LootService>(Lifetime.Singleton).AsSelf();
            builder.Register<EnemyDeathLootHandler>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            // Input
            builder.Register<InputDispatcher>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            // Interaction & Turn
            builder.Register<InteractionLockService>(Lifetime.Singleton).As<IInteractionLock>().AsSelf();
            builder.Register<ClickIntentResolver>(Lifetime.Singleton).As<IClickIntentResolver>();
            builder.Register<InteractionController>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<TurnController>(Lifetime.Singleton).As<ITurnController>();

            // Stage
            builder.Register<StageQueryService>(Lifetime.Singleton).As<IStageQueryService>();
            builder.Register<AttackQueryService>(Lifetime.Singleton).As<IAttackQueryService>();
            builder.Register<StageHighlightRenderer>(Lifetime.Singleton).As<IStageHighlightRenderer>();
            builder.Register<StageService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<StageReloadService>(Lifetime.Singleton).AsSelf();
            builder.Register<GameShutdownCleanupService>(Lifetime.Singleton);

            // Stage phases (PrepareZoneCachePhase, PreparePlacementPhase, PrepareService need Unity — register in Unity)
            builder.Register<BoardSpawnPhase>(Lifetime.Transient);
            builder.Register<GameplayInitPhase>(Lifetime.Transient);
            builder.Register<BattleDuelPhase>(Lifetime.Transient);

            // Figures spawn
            builder.Register<SpawnPatternParser>(Lifetime.Singleton);
            builder.Register<DuelFiguresSpawnProvider>(Lifetime.Singleton);
            builder.Register<EmptyFiguresSpawnProvider>(Lifetime.Singleton);
            builder.Register<IFiguresSpawnProviderFactory, FiguresSpawnProviderFactory>(Lifetime.Singleton);

            // Board appear
            builder.Register<BoardAppearAnimationFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IBoardAppearAnimationStrategy>>(new IBoardAppearAnimationStrategy[]
                {
                    new BoardNoneAppearStrategy(),
                    new BoardRainDropAppearStrategy()
                });

            // Movement
            builder.Register<MovementStrategyFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IMovementStrategy>>(new IMovementStrategy[]
                {
                    new PawnMovement(),
                    new KnightMovement(),
                    new RookMovement(),
                    new BishopMovement(),
                    new QueenMovement()
                });

            builder.Register<FigureStatsFactory>(Lifetime.Singleton).As<IFigureStatsFactory>().AsSelf();
            builder.Register<AttackResolver>(Lifetime.Singleton).As<IAttackResolver>();
            builder.Register<TargetingService>(Lifetime.Singleton).As<ITargetingService>();
            builder.Register<EngagementRuleService>(Lifetime.Singleton).As<IEngagementRuleService>();

            // Attack strategies
            builder.Register<AttackStrategyFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IAttackStrategy>>(new IAttackStrategy[]
                {
                    new SimpleAttack(),
                    new RangedAttack(),
                    new SplashAttack(),
                    new PierceAttack()
                });

            // Combat
            builder.Register<PassiveTriggerService>(Lifetime.Singleton);
            builder.Register<DamagePipeline>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IDamageModifier>>(new IDamageModifier[] { new CritDamageModifier() })
                .As<IDamagePipeline>();
            builder.Register<CombatResolver>(Lifetime.Singleton);
            builder.Register<CombatVisualPlanner>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IVisualEventMapper>>(new IVisualEventMapper[]
                {
                    new AttackVisualEventMapper(),
                    new ProjectileVisualEventMapper(),
                    new ProjectileImpactEventMapper(),
                    new CleanupProjectileEventMapper(),
                    new ProjectileHitApplyEventMapper(),
                    new BeamVisualEventMapper(),
                    new WaveVisualEventMapper(),
                    new DamageVisualEventMapper(),
                    new HealVisualEventMapper(),
                    new PushVisualEventMapper(),
                    new MoveVisualEventMapper(),
                    new DeathVisualEventMapper(),
                    new LootVisualEventMapper()
                })
                .As<ICombatVisualPlanner>();

            // Turn
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
            builder.Register<TurnExecutor>(Lifetime.Singleton).As<ITurnExecutor>();
            builder.Register<BonusMoveController>(Lifetime.Singleton).As<IBonusMoveController>();
            builder.Register<BonusMoveSession>(Lifetime.Singleton).As<IBonusMoveSession>();
            builder.Register<TurnSystem>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            // Domain services
            builder.Register<BoardSpawnService>(Lifetime.Singleton);
            builder.Register<FigureSpawnService>(Lifetime.Singleton);
            builder.Register<DamageService>(Lifetime.Singleton);
            builder.Register<MovementService>(Lifetime.Singleton);
            builder.Register<UIService>(Lifetime.Singleton);
        }
    }
}
