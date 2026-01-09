using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnManager
    {
        private readonly BoardGrid _grid;

        public TurnPhase Phase { get; private set; }

        public TurnManager(BoardGrid grid)
        {
            _grid = grid;
            Phase = TurnPhase.PlayerTurn;
        }

        public void StartTurn()
        {
            ApplyCellEffects();
        }

        private void ApplyCellEffects()
        {
            for (int r = 0; r < _grid.Height; r++)
            {
                for (int c = 0; c < _grid.Width; c++)
                {
                    BoardCell cell =
                        _grid.GetBoardCell(
                            new GridPosition(r, c));

                    cell.Effects.OnTurnStart(cell);
                }
            }
        }

        public void EndTurn()
        {
            Phase = Phase switch
            {
                TurnPhase.PlayerTurn => TurnPhase.EnemyTurn,
                TurnPhase.EnemyTurn => TurnPhase.PlayerTurn,
                _ => Phase
            };
        }
    }
}