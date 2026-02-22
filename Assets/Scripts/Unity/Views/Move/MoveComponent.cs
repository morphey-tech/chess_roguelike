using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;

namespace Project.Unity
{
    public abstract class MoveComponent : MonoBehaviour
    {
        protected BoardGrid Grid { get; private set; }
        protected IFigureView Figure { get; private set; }
        
        public void Init(IFigureView figure, BoardGrid board)
        {
            Figure = figure;
            Grid = board;
        }

        public abstract List<BoardCell> GetPossibleMoveCells();
    }
}
