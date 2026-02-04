using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Turn.BonusMove
{
    public readonly struct BonusMoveStartedMessage
    {
        public Figure Actor { get; }

        public BonusMoveStartedMessage(Figure actor)
        {
            Actor = actor;
        }
    }
}
