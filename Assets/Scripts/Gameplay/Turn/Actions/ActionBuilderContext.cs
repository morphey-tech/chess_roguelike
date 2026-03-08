using MessagePipe;
using Project.Core.Core.Logging;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Attack.Rules;
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
        public TriggerService TriggerService { get; }
        public VisualPipeline VisualPipeline { get; }
        public IPublisher<FigureDeathMessage> DeathPublisher { get; }
        public IPublisher<FigureAttackStartedMessage> AttackStartedPublisher { get; }
        public LootService LootService { get; }
        public DamageApplier DamageApplier { get; }
        public IFigureLifeService FigureLifeService { get; }
        public Turn.ActionContextAccessor ContextAccessor { get; }
        public ILogService LogService { get; }
        public IAttackQueryService AttackQueryService { get; }
        public AttackRuleService AttackRuleService { get; }

        public ActionBuilderContext(
            MovementService movementService,
            AttackStrategyFactory attackFactory,
            IAttackResolver attackResolver,
            CombatResolver combatResolver,
            ICombatVisualPlanner visualPlanner,
            TriggerService triggerService,
            VisualPipeline visualPipeline,
            IPublisher<FigureDeathMessage> deathPublisher,
            IPublisher<FigureAttackStartedMessage> attackStartedPublisher,
            LootService lootService,
            DamageApplier damageApplier,
            IFigureLifeService figureLifeService,
            ActionContextAccessor contextAccessor,
            ILogService logService,
            IAttackQueryService attackQueryService,
            AttackRuleService attackRuleService)
        {
            MovementService = movementService;
            AttackFactory = attackFactory;
            AttackResolver = attackResolver;
            CombatResolver = combatResolver;
            VisualPlanner = visualPlanner;
            TriggerService = triggerService;
            VisualPipeline = visualPipeline;
            DeathPublisher = deathPublisher;
            AttackStartedPublisher = attackStartedPublisher;
            LootService = lootService;
            DamageApplier = damageApplier;
            FigureLifeService = figureLifeService;
            ContextAccessor = contextAccessor;
            LogService = logService;
            AttackQueryService = attackQueryService;
            AttackRuleService = attackRuleService;
        }
    }
}
