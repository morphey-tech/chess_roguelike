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
    public interface IActionBuilderContext
    {
        MovementService MovementService { get; }
        AttackStrategyFactory AttackFactory { get; }
        IAttackResolver AttackResolver { get; }
        CombatResolver CombatResolver { get; }
        ICombatVisualPlanner VisualPlanner { get; }
        TriggerService TriggerService { get; }
        VisualPipeline VisualPipeline { get; }
        IPublisher<FigureAttackMessage> AttackPublisher { get; }
        IPublisher<FigureDiedMessage> DiePublisher { get; }
        LootService LootService { get; }
        DamageApplier DamageApplier { get; }
        IFigureLifeService FigureLifeService { get; }
        Turn.ActionContextAccessor ContextAccessor { get; }
        ILogService LogService { get; }
        IAttackQueryService AttackQueryService { get; }
        AttackRuleService AttackRuleService { get; }
    }
}