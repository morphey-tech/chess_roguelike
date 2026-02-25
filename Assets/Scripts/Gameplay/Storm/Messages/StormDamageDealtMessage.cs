namespace Project.Gameplay.ShrinkingZone.Messages
{
    /// <summary>
    /// Команда: нанесён урон (для активации зоны)
    /// </summary>
    public readonly struct StormDamageDealtMessage
    {
        public readonly int Turn;

        public StormDamageDealtMessage(int turn)
        {
            Turn = turn;
        }
    }
}