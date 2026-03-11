using System.Collections.Generic;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Loot;

namespace Project.Gameplay.Gameplay.Combat.Effects
{
    /// <summary>
    /// Context provided to combat effects during execution.
    /// 
    /// DESIGN RULES:
    /// - Effects record visual events, never call presenters directly
    /// - Effects can add new combat effects via AddEffect()
    /// - Visual commands are executed AFTER all combat effects complete
    /// - NO presenter references - commands receive presenters at execution time
    /// </summary>
    public sealed class CombatEffectContext : ICombatEffectSink
    {
        public ActionContext ActionContext { get; }
        public BoardGrid Grid { get; }
        public IPublisher<FigureAttackMessage> AttackPublisher { get; }
        public TriggerService TriggerService { get; }
        public LootService LootService { get; }
        public DamageApplier DamageApplier { get; }
        public IFigureLifeService FigureLifeService { get; }
        public ILogger Logger { get; }
        public List<ICombatVisualEvent> VisualEvents { get; }
        public List<ICombatEffect> PendingEffects { get; } = new();

        public CombatEffectContext(
            ActionContext actionContext,
            BoardGrid grid,
            IPublisher<FigureAttackMessage> attackPublisher,
            TriggerService triggerService,
            LootService lootService,
            DamageApplier damageApplier,
            IFigureLifeService figureLifeService,
            List<ICombatVisualEvent> visualEvents,
            ILogger logger)
        {
            ActionContext = actionContext;
            Grid = grid;
            AttackPublisher = attackPublisher;
            TriggerService = triggerService;
            LootService = lootService;
            DamageApplier = damageApplier;
            FigureLifeService = figureLifeService;
            VisualEvents = visualEvents;
            Logger = logger;
        }
        
        public void AddEffect(ICombatEffect effect)
        {
            PendingEffects.Add(effect);
        }

        public void AddVisualEvent(ICombatVisualEvent? visualEvent)
        {
            if (visualEvent != null)
            {
                VisualEvents.Add(visualEvent);
            }
        }
    }
}
