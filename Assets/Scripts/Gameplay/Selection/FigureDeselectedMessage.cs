using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Selection
{
    public readonly struct FigureDeselectedMessage
    {
        public readonly Figure Figure;
        
        public FigureDeselectedMessage(Figure figure)
        {
            Figure = figure;
        }
    }
}