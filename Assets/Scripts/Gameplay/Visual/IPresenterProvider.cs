using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Provides access to presenters for visual commands.
    /// Commands don't hold presenter references directly.
    /// </summary>
    public interface IPresenterProvider
    {
        IFigurePresenter Figures { get; }
    }
}
