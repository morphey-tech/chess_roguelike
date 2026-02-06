using Project.Gameplay.Gameplay.Board;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Prepare;
using VContainer;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Default implementation of IPresenterProvider.
    /// </summary>
    public sealed class PresenterProvider : IPresenterProvider
    {
        public IBoardPresenter Board { get; }
        public IFigurePresenter Figures { get; }
        public IPreparePresenter Prepare { get; }

        [Inject]
        private PresenterProvider(
            IFigurePresenter figurePresenter,
            IBoardPresenter board,
            IPreparePresenter prepare)
        {
            Figures = figurePresenter;
            Board = board;
            Prepare = prepare;
        }
    }
}
