using System.Collections.Generic;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Cells
{
    public class CellEffectContainer
    {
        private readonly List<CellEffect> _effects = new();

        public void Add(CellEffect effect)
        {
            _effects.Add(effect);
        }

        public void OnEnter(BoardCell cell)
        {
            foreach (CellEffect effect in _effects)
            {
                effect.OnEnter(cell);
            }
        }

        public void OnTurnStart(BoardCell cell)
        {
            foreach (CellEffect effect in _effects)
            {
                effect.OnTurnStart(cell);
            }
        }
    }
}