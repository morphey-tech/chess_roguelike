using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Loot;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Turn.Actions
{
    /// <summary>
    /// Implementation of IActionBuilderContext providing all services needed to build actions.
    /// </summary>
    public sealed class ActionBuilderContext : IActionBuilderContext
    {
        public MovementService MovementService { get; }
        public AttackStrategyFactory AttackFactory { get; }
        public IAttackResolver AttackResolver { get; }
        public CombatResolver CombatResolver { get; }
        public ICombatVisualPlanner VisualPlanner { get; }
        public PassiveTriggerService Passives { get; }
        public VisualPipeline VisualPipeline { get; }
        public IPublisher<FigureDeathMessage> DeathPublisher { get; }
        public LootService LootService { get; }
        public DamageApplier DamageApplier { get; }
        public IFigureLifeService FigureLifeService { get; }
        public Turn.ActionContextAccessor ContextAccessor { get; }
        public ILogService LogService { get; }
        public IAttackQueryService AttackQueryService { get; }

        public ActionBuilderContext(
            MovementService movementService,
            AttackStrategyFactory attackFactory,
            IAttackResolver attackResolver,
            CombatResolver combatResolver,
            ICombatVisualPlanner visualPlanner,
            PassiveTriggerService passives,
            VisualPipeline visualPipeline,
            IPublisher<FigureDeathMessage> deathPublisher,
            LootService lootService,
            DamageApplier damageApplier,
            IFigureLifeService figureLifeService,
            Turn.ActionContextAccessor contextAccessor,
            ILogService logService,
            IAttackQueryService attackQueryService)
        {
            MovementService = movementService;
            AttackFactory = attackFactory;
            AttackResolver = attackResolver;
            CombatResolver = combatResolver;
            VisualPlanner = visualPlanner;
            Passives = passives;
            VisualPipeline = visualPipeline;
            DeathPublisher = deathPublisher;
            LootService = lootService;
            DamageApplier = damageApplier;
            FigureLifeService = figureLifeService;
            ContextAccessor = contextAccessor;
            LogService = logService;
            AttackQueryService = attackQueryService;
        }
    }
}
