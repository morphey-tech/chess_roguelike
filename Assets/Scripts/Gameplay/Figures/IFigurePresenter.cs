using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Figures
{
    public interface IFigurePresenter
    {
        UniTask CreateFigure(int figureId, string typeId, GridPosition pos, Team team);
        void MoveFigure(int figureId, GridPosition to);
        void RemoveFigure(int figureId);
        void PlayAttack(int figureId, GridPosition target);
        void Clear();
    }
}
