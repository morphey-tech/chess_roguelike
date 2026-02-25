namespace Project.Gameplay.ShrinkingZone.Messages
{
    /// <summary>
    /// Команда: начался ход
    /// </summary>
    public readonly struct StormTurnStartedMessage
    {
        public readonly int Turn;

        public StormTurnStartedMessage(int turn)
        {
            Turn = turn;
        }
    }
}