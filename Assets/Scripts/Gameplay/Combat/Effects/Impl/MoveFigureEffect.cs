using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Moves the attacker to a new position (e.g., retreat after attack).
    /// Note: grid state is already updated during passive execution.
    /// </summary>
    public sealed class MoveFigureEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.Movement;
        public int OrderInPhase => 10; // After push

        private readonly Figure _figure;
        private readonly GridPosition _newPosition;
        private readonly bool _updateActionContextFrom;

        public MoveFigureEffect(Figure figure, GridPosition newPosition, bool updateActionContextFrom = false)
        {
            _figure = figure;
            _newPosition = newPosition;
            _updateActionContextFrom = updateActionContextFrom;
        }

        public UniTask ApplyAsync(CombatEffectContext context)
        {
            context.Logger.Info($"{_figure} moved to ({_newPosition.Row}, {_newPosition.Column})");
            context.FigurePresenter.MoveFigure(_figure.Id, _newPosition);
            
            if (_updateActionContextFrom)
            {
                context.ActionContext.From = _newPosition;
            }
            
            return UniTask.CompletedTask;
        }
    }
}
