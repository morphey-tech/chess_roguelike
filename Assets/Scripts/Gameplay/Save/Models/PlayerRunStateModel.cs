using System;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Save.Models
{
    /// <summary>
    /// Single source of truth for run state.
    /// Stores FACTS, not phases or UI state.
    /// </summary>
    [Serializable]
    public sealed class PlayerRunStateModel
    {
        public string StageId { get; set; }
        public int CurrentStageIndex { get; set; }
        public int KingHp { get; set; }
        public int Seed { get; set; }
        public List<FigureState> Figures { get; set; } = new();

        public IReadOnlyList<FigureState> FiguresInHand =>
            Figures.Where(u => u.Location.Type == FigureLocationType.Hand).ToList();

        public IReadOnlyList<FigureState> FiguresOnBoard =>
            Figures.Where(u => u.Location.Type == FigureLocationType.Board).ToList();

        public FigureState? GetFigure(string unitId)
        {
            return Figures.Find(u => u.Id == unitId);
        }

        public void PlaceOnBoard(string unitId, GridPosition position)
        {
            FigureState? figure = GetFigure(unitId);
            if (figure != null)
            {
                figure.Location = FigureLocation.OnBoard(position);
            }
        }

        public void MarkDead(string unitId)
        {
            FigureState? figure = GetFigure(unitId);
            if (figure != null)
            {
                figure.Location = FigureLocation.Dead();
            }
        }

        public void AddFigure(string typeId)
        {
            string id = Guid.NewGuid().ToString("N")[..8];
            Figures.Add(new FigureState(id, typeId));
        }
    }
}
