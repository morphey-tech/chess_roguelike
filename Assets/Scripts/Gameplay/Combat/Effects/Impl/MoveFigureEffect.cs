using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Queues move visual command for a figure (e.g., retreat after attack).
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

        public void Apply(CombatEffectContext context)
        {
            context.Logger.Info($"{_figure} moved to ({_newPosition.Row}, {_newPosition.Column})");
            
            context.AddVisualEvent(new MoveVisualEvent(_figure.Id, _newPosition));
            
            // Domain: Update context if needed
            if (_updateActionContextFrom)
            {
                context.ActionContext.From = _newPosition;
            }
        }
    }
}
