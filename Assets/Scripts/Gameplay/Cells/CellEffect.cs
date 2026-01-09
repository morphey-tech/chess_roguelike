using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Cells
{
    public abstract  class CellEffect
    {
        public abstract void OnEnter(BoardCell cell);
        public abstract void OnTurnStart(BoardCell cell);
    }
}