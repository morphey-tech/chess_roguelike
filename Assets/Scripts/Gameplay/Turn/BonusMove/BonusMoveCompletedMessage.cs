using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Turn.BonusMove
{
    public readonly struct BonusMoveCompletedMessage
    {
        public Figure Actor { get; }

        public BonusMoveCompletedMessage(Figure actor)
        {
            Actor = actor;
        }
    }
}
