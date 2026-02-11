using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Мгновенная смерть без урона (например execute). Всё делегирует UnitLifeService.
    /// </summary>
    public sealed class KillEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.Death;
        public int OrderInPhase => 0;

        private readonly Figure _victim;
        private readonly BoardCell _cell;

        public KillEffect(Figure victim, BoardCell cell, string deathReason = null)
        {
            _victim = victim;
            _cell = cell;
        }

        public void Apply(CombatEffectContext context)
        {
            context.FigureLifeService.HandleDeathFromCombat(context, _victim, _cell);
        }
    }
}
