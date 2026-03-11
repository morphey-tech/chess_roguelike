using System.Collections.Generic;
using Project.Core.Core.Filters;
using Project.Core.Core.Random;
using Project.Core.Core.Storm.Core;
using Project.Core.Core.Storm.Strategies;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Attack.Rules;
using Project.Gameplay.Gameplay.Attack.Strategies;
using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Board.Capacity;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Filters;
using Project.Gameplay.Gameplay.Input;
using Project.Gameplay.Gameplay.Interaction;
using Project.Gameplay.Gameplay.Loot;
using Project.Gameplay.Gameplay.Movement;
using Project.Gameplay.Gameplay.Movement.Strategies;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Shutdown;
using Project.Gameplay.Gameplay.Stage;
using Project.Gameplay.Gameplay.Stage.Flow;
using Project.Gameplay.Gameplay.Stage.Phase;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Turn.Actions;
using Project.Gameplay.Gameplay.Turn.Actions.Builders;
using Project.Gameplay.Gameplay.Turn.BonusMove;
using Project.Gameplay.Gameplay.Turn.Conditions;
using Project.Gameplay.Gameplay.Turn.Conditions.Impl;
using Project.Gameplay.Gameplay.Turn.Execution;
using Project.Gameplay.ShrinkingZone;
using VContainer;
using VContainer.Unity;

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
            // Core services
            builder.Register<RandomService>(Lifetime.Singleton)
                .As<IRandomService>();

            // Run & Stage
            builder.Register<RunHolder>(Lifetime.Singleton);
            builder.Register<RunFactory>(Lifetime.Singleton);
            builder.Register<StageFactory>(Lifetime.Singleton);
            builder.Register<StagePhaseFactory>(Lifetime.Singleton);

            // Loot (ILootPresenter регистрируется в Unity в ConfigureViews вместе с другими презентерами)
            builder.Register<LootService>(Lifetime.Singleton).AsSelf();
            builder.RegisterEntryPoint<EnemyDeathLootHandler>();

            // Input
            builder.RegisterEntryPoint<InputDispatcher>();

            // Interaction & Turn
            builder.Register<InteractionLockService>(Lifetime.Singleton).As<IInteractionLock>().AsSelf();
            builder.Register<ClickIntentResolver>(Lifetime.Singleton).As<IClickIntentResolver>();
            builder.Register<InteractionController>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<TurnExecutionFlow>(Lifetime.Singleton).As<ITurnController>();

            //Storm
            builder.Register<LayerRingStrategy>(Lifetime.Singleton)
                .As<IStormStrategy>();
            builder.Register<StormSystemFactory>(Lifetime.Singleton)
                .As<IStormSystemFactory>();
            builder.Register<StormCellEvaluator>(Lifetime.Singleton)
                .AsSelf()
                .As<IStormCellEvaluator>();
            builder.Register<StormBattleService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<StormInitService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<StormHighlightRenderer>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register<StormDamageService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            
            // Stage
            builder.Register<StageQueryService>(Lifetime.Singleton).As<IStageQueryService>();
            builder.Register<AttackQueryService>(Lifetime.Singleton).As<IAttackQueryService>();
            builder.Register<StageHighlightRenderer>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.RegisterEntryPoint<StageService>();
            builder.Register<StageRunStateResetService>(Lifetime.Singleton);
            builder.Register<StageRuntimeResetService>(Lifetime.Singleton);
            builder.Register<StageCacheResetService>(Lifetime.Singleton);
            builder.Register<StageReloadService>(Lifetime.Singleton).AsSelf();
            builder.Register<RunTransitionService>(Lifetime.Singleton).As<IRunTransitionService>();
            builder.Register<RunFlowService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
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
            builder.Register<FigureRegistry>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            // Movement
            builder.Register<MovementStrategyFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IMovementStrategy>>(new IMovementStrategy[]
                {
                    new PawnMovement(),
                    new KnightMovement(),
                    new SplasherMovement()
                });

            builder.Register<FigureStatsFactory>(Lifetime.Singleton).As<IFigureStatsFactory>().AsSelf();
            builder.Register<AttackResolver>(Lifetime.Singleton).As<IAttackResolver>();
            builder.Register<TargetingService>(Lifetime.Singleton).As<ITargetingService>();
            builder.Register<EngagementRuleService>(Lifetime.Singleton).As<IEngagementRuleService>();

            // Attack rules
            builder.Register<RangeRule>(Lifetime.Transient).As<IAttackRule>();
            builder.Register<TauntRule>(Lifetime.Transient).As<IAttackRule>();
            builder.Register<DisarmRule>(Lifetime.Transient).As<IAttackRule>();
            builder.Register<StealthRule>(Lifetime.Transient).As<IAttackRule>();
            builder.Register<DesperationRule>(Lifetime.Transient).As<IAttackRule>();
            builder.Register<AttackRuleService>(Lifetime.Singleton);

            // Attack strategies
            builder.Register<AttackStrategyFactory>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IAttackStrategy>>(new IAttackStrategy[]
                {
                    new SimpleAttack(),
                    new RangedAttack(),
                    new DiagonalAttack(),
                    new PawnAttack()
                });

            // Combat
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
            
            // Action builders (new system)
            builder.Register<ActionBuilderRegistry>(Lifetime.Singleton)
                .WithParameter<IEnumerable<IActionBuilder>>(new IActionBuilder[]
                {
                    new MoveActionBuilder(),
                    new AttackActionBuilder(),
                    new MoveThenAttackActionBuilder(),
                    new MoveToTargetActionBuilder(),
                    new MoveToKilledTargetActionBuilder()
                });
            // SequentialActionBuilder registered in GameLifetimeScope via RegisterBuildCallback
            // ActionBuilderContext registered in GameLifetimeScope (after VisualPipeline, ActionContextAccessor, etc.)
            
            builder.Register<TurnPatternFactory>(Lifetime.Singleton);
            builder.Register<TurnPatternResolver>(Lifetime.Singleton);
            builder.Register<TurnExecutor>(Lifetime.Singleton).As<ITurnExecutor>();
            builder.Register<BonusMoveController>(Lifetime.Singleton).As<IBonusMoveController>();
            builder.Register<BonusMoveSession>(Lifetime.Singleton).As<IBonusMoveSession>();
            builder.Register<TurnService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            // Domain services
            builder.Register<BoardSpawnService>(Lifetime.Singleton);
            builder.Register<FigureSpawnService>(Lifetime.Singleton);
            builder.Register<MovementService>(Lifetime.Singleton);
            builder.Register<PassiveFactory>(Lifetime.Singleton);
        }
    }
}
