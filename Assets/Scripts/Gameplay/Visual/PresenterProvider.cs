using Project.Gameplay.Gameplay.Figures;
using VContainer;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Default implementation of IPresenterProvider.
    /// </summary>
    public sealed class PresenterProvider : IPresenterProvider
    {
        public IFigurePresenter Figures { get; }

        [Inject]
        private PresenterProvider(IFigurePresenter figurePresenter)
        {
            Figures = figurePresenter;
        }
    }
}
