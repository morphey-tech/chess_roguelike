using MessagePipe;
using Project.Core.Core.Logging;
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
    /// Builds an ICombatAction from config. Registered by action type (e.g. "move", "attack", "move_then_attack").
    /// Keeps config-driven patterns: TurnPatternFactory uses builders to create actions from ActionConfig.
    /// </summary>
    public interface IActionBuilder
    {
        string ActionType { get; }

        ICombatAction Build(ActionConfig config, IActionBuilderContext builderContext);
    }

    /// <summary>
    /// Services and factories the builder needs to construct actions (MovementService, AttackFactory, etc.).
    /// </summary>
    public interface IActionBuilderContext
    {
        MovementService MovementService { get; }
        AttackStrategyFactory AttackFactory { get; }
        IAttackResolver AttackResolver { get; }
        CombatResolver CombatResolver { get; }
        ICombatVisualPlanner VisualPlanner { get; }
        PassiveTriggerService Passives { get; }
        VisualPipeline VisualPipeline { get; }
        IPublisher<FigureDeathMessage> DeathPublisher { get; }
        LootService LootService { get; }
        DamageApplier DamageApplier { get; }
        IFigureLifeService FigureLifeService { get; }
        Turn.ActionContextAccessor ContextAccessor { get; }
        ILogService LogService { get; }
        IAttackQueryService AttackQueryService { get; }
        AttackRuleService AttackRuleService { get; }
    }
}
