using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Dumb presenter that executes visual commands.
    /// All methods return UniTask to allow awaiting animations.
    /// Presenter has no game logic - only visuals.
    /// </summary>
    public interface IFigurePresenter
    {
        UniTask CreateFigure(Figure figure, string assetKey, GridPosition pos, Team team);
        UniTask MoveFigureAsync(int figureId, GridPosition to);
        UniTask RemoveFigureAsync(int figureId);
        UniTask PlayAttackAsync(int figureId, GridPosition target);
        UniTask PlayDamageEffectAsync(int figureId);
        UniTask PlayHealEffectAsync(int figureId);
        UniTask PlayDeathEffectAsync(int figureId);
        UniTask PlayPushEffectAsync(int figureId, GridPosition from, GridPosition to);
        void Clear();
    }
}
