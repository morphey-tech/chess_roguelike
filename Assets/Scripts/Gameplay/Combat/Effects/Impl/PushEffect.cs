using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Visual effect for pushing a figure to a new position.
    /// Note: grid state is already updated during passive execution.
    /// </summary>
    public sealed class PushEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.Movement;
        public int OrderInPhase => 0;

        private readonly Figure _target;
        private readonly GridPosition _newPosition;

        public PushEffect(Figure target, GridPosition newPosition)
        {
            _target = target;
            _newPosition = newPosition;
        }

        public UniTask ApplyAsync(CombatEffectContext context)
        {
            context.Logger.Info($"{_target} pushed to ({_newPosition.Row}, {_newPosition.Column})");
            context.FigurePresenter.MoveFigure(_target.Id, _newPosition);
            return UniTask.CompletedTask;
        }
    }
}
