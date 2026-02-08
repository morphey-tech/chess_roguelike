using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Handles death: domain removes figure from grid, queues death visual, publishes event.
    /// </summary>
    public sealed class KillEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.Death;
        public int OrderInPhase => 0;

        private readonly Figure _victim;
        private readonly BoardCell _cell;
        private readonly string _deathReason;

        public KillEffect(Figure victim, BoardCell cell, string deathReason = null)
        {
            _victim = victim;
            _cell = cell;
            _deathReason = deathReason;
        }

        public void Apply(CombatEffectContext context)
        {
            string reason = string.IsNullOrEmpty(_deathReason) ? "" : $" ({_deathReason})";
            context.Logger.Info($"{_victim} died{reason}!");
            
            // Domain: Remove from grid
            _cell?.RemoveFigure();
            
            context.AddVisualEvent(new DeathVisualEvent(_victim.Id, _deathReason));
            
            // Domain: Publish event
            context.DeathPublisher.Publish(new FigureDeathMessage(_victim.Id, _victim.Team));
            
            // Domain: Update context
            context.ActionContext.LastAttackKilledTarget = true;
        }
    }
}
