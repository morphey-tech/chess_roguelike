using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Handles death: removes figure from grid/presenter, publishes death event.
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

        public UniTask ApplyAsync(CombatEffectContext context)
        {
            string reason = string.IsNullOrEmpty(_deathReason) ? "" : $" ({_deathReason})";
            context.Logger.Info($"{_victim} died{reason}!");
            
            _cell?.RemoveFigure();
            context.FigurePresenter.RemoveFigure(_victim.Id);
            context.DeathPublisher.Publish(new FigureDeathMessage(_victim.Id, _victim.Team));
            
            context.ActionContext.LastAttackKilledTarget = true;
            
            return UniTask.CompletedTask;
        }
    }
}
