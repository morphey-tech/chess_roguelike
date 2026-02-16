using System.Collections.Generic;
using System.Threading;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Prepare
{
    public sealed class PrepareContext
    {
        public PlayerRunStateModel RunState { get; }
        public BoardGrid Grid { get; }
        public IPreparePlacementRules Rules { get; }
        public PrepareState State { get; }
        public CancellationToken CancellationToken { get; }
        
        public HashSet<GridPosition> AvailablePlacementPositions { get; } = new();

        public string? PreviousSelectedId { get; set; }
        public bool IsInputReady { get; set; }

        public PrepareContext(
            PlayerRunStateModel runState,
            BoardGrid grid,
            IPreparePlacementRules rules,
            PrepareState state,
            CancellationToken cancellationToken)
        {
            RunState = runState;
            Grid = grid;
            Rules = rules;
            State = state;
            CancellationToken = cancellationToken;
        }

        public FigureState? GetSelectedFigure()
        {
            return State.SelectedFigureId != null ? RunState.GetFigure(State.SelectedFigureId) : null;
        }
    }
}
