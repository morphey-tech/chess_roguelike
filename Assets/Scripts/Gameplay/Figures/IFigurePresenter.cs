using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Figures
{
    public interface IFigurePresenter
    {
        UniTask CreateFigure(Figure figure, string assetKey, GridPosition pos, Team team);
        void MoveFigure(int figureId, GridPosition to);
        void RemoveFigure(int figureId);
        void PlayAttack(int figureId, GridPosition target);
        void PlayDamageEffect(int figureId);
        void PlayDeathEffect(int figureId);
        void Clear();
    }
}
