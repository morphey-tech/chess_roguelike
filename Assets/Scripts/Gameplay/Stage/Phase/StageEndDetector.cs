using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    public sealed class StageEndDetector
    {
        private readonly BoardGrid _board;

        public StageEndDetector(BoardGrid board)
        {
            _board = board;
        }

        public bool IsVictory()
        {
            foreach (var figure in _board.GetAllFigures())
            {
                if (figure.Team == Team.Enemy)
                    return false;
            }
            return true;
        }

        public bool IsDefeat()
        {
            foreach (var figure in _board.GetAllFigures())
            {
                if (figure.Team == Team.Player)
                    return false;
            }
            return true;
        }
    }
}
