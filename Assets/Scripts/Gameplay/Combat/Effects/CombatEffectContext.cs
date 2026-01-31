using System.Collections.Generic;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Combat.Effects
{
    /// <summary>
    /// Context provided to combat effects during execution.
    /// Contains all services needed to apply visual/event effects.
    /// Effects can add new effects via AddEffect().
    /// </summary>
    public sealed class CombatEffectContext : ICombatEffectSink
    {
        public ActionContext ActionContext { get; }
        public BoardGrid Grid { get; }
        public IFigurePresenter FigurePresenter { get; }
        public IPublisher<FigureDeathMessage> DeathPublisher { get; }
        public PassiveTriggerService Passives { get; }
        public ILogger Logger { get; }
        
        /// <summary>
        /// Effects added during Apply. Processed after current effect.
        /// </summary>
        public List<ICombatEffect> PendingEffects { get; } = new();

        public CombatEffectContext(
            ActionContext actionContext,
            BoardGrid grid,
            IFigurePresenter figurePresenter,
            IPublisher<FigureDeathMessage> deathPublisher,
            PassiveTriggerService passives,
            ILogger logger)
        {
            ActionContext = actionContext;
            Grid = grid;
            FigurePresenter = figurePresenter;
            DeathPublisher = deathPublisher;
            Passives = passives;
            Logger = logger;
        }
        
        public void AddEffect(ICombatEffect effect) => PendingEffects.Add(effect);
    }
}
