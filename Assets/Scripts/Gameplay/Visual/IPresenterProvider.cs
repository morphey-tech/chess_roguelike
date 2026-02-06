using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Prepare;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Provides access to presenters for visual commands.
    /// Commands don't hold presenter references directly.
    /// </summary>
    public interface IPresenterProvider
    {
        IBoardPresenter Board { get; }
        IFigurePresenter Figures { get; }
        IPreparePresenter Prepare { get; }
    }
}
