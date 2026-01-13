using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Turn
{
    public readonly struct TurnChangedMessage
    {
        public readonly Team CurrentTeam;
        public readonly int TurnNumber;

        public TurnChangedMessage(Team currentTeam, int turnNumber)
        {
            CurrentTeam = currentTeam;
            TurnNumber = turnNumber;
        }
    }
}