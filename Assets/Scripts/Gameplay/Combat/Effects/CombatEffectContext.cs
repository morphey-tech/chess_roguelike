using System.Collections.Generic;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Visual.Commands;

namespace Project.Gameplay.Gameplay.Combat.Effects
{
    /// <summary>
    /// Context provided to combat effects during execution.
    /// 
    /// DESIGN RULES:
    /// - Effects add visual commands via Visuals sink, never call presenters directly
    /// - Effects can add new combat effects via AddEffect()
    /// - Visual commands are executed AFTER all combat effects complete
    /// - NO presenter references - commands receive presenters at execution time
    /// </summary>
    public sealed class CombatEffectContext : ICombatEffectSink
    {
        public ActionContext ActionContext { get; }
        public BoardGrid Grid { get; }
        public IPublisher<FigureDeathMessage> DeathPublisher { get; }
        public PassiveTriggerService Passives { get; }
        public ILogger Logger { get; }
        
        /// <summary>
        /// Sink for visual commands. Effects add commands here.
        /// Commands are executed after all effects complete.
        /// Commands receive presenters at execution time, not creation time.
        /// </summary>
        public IVisualCommandSink Visuals { get; }
        
        /// <summary>
        /// Effects added during Apply. Processed after current effect.
        /// </summary>
        public List<ICombatEffect> PendingEffects { get; } = new();

        public CombatEffectContext(
            ActionContext actionContext,
            BoardGrid grid,
            IPublisher<FigureDeathMessage> deathPublisher,
            PassiveTriggerService passives,
            IVisualCommandSink visuals,
            ILogger logger)
        {
            ActionContext = actionContext;
            Grid = grid;
            DeathPublisher = deathPublisher;
            Passives = passives;
            Visuals = visuals;
            Logger = logger;
        }
        
        public void AddEffect(ICombatEffect effect) => PendingEffects.Add(effect);
    }
}
