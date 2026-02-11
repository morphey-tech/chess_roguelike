using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Temporary phase state for Prepare.
    /// Created per phase, not stored in DI.
    /// This is UX logic, not world state.
    /// </summary>
    public sealed class PrepareState
    {
        public bool IsActive { get; private set; }
        public string? SelectedFigureId { get; private set; }
        public bool IsCompleted => _availableFigures.Count == 0;

        private readonly HashSet<string> _availableFigures;

        public PrepareState(IEnumerable<FigureState> figuresInHand)
        {
            _availableFigures = new HashSet<string>(figuresInHand.Select(u => u.Id));
        }

        public void Start()
        {
            IsActive = true;
            SelectedFigureId = null;
        }

        public void Select(string figureId)
        {
            if (!_availableFigures.Contains(figureId))
            {
                return;
            }
            SelectedFigureId = figureId;
        }

        public void ClearSelection()
        {
            SelectedFigureId = null;
        }

        public void OnPlaced(string figureId)
        {
            _availableFigures.Remove(figureId);
            SelectedFigureId = null;
        }

        public void Restore(string figureId)
        {
            _availableFigures.Add(figureId);
            SelectedFigureId = null;
        }

        public void Complete()
        {
            IsActive = false;
            SelectedFigureId = null;
        }

    }
}
