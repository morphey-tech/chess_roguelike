using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Queues push visual command for a figure.
    /// Note: grid state is already updated during passive execution.
    /// </summary>
    public sealed class PushEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.Movement;
        public int OrderInPhase => 0;

        private readonly Figure _target;
        private readonly GridPosition _fromPosition;
        private readonly GridPosition _toPosition;

        public PushEffect(Figure target, GridPosition fromPosition, GridPosition toPosition)
        {
            _target = target;
            _fromPosition = fromPosition;
            _toPosition = toPosition;
        }

        public void Apply(CombatEffectContext context)
        {
            context.Logger.Info($"{_target} pushed from ({_fromPosition.Row}, {_fromPosition.Column}) to ({_toPosition.Row}, {_toPosition.Column})");
            context.AddVisualEvent(new PushVisualEvent(_target.Id, _fromPosition, _toPosition));
        }
    }
}
