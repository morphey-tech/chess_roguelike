using Project.Core.Core.ShrinkingZone.Core;

namespace Project.Gameplay.ShrinkingZone.Messages
{
    /// <summary>
    /// Команда: начать бой (инициализация зоны)
    /// </summary>
    public readonly struct ZoneBattleStartedMessage
    {
    }

    /// <summary>
    /// Команда: начался ход
    /// </summary>
    public readonly struct ZoneTurnStartedMessage
    {
        public readonly int Turn;

        public ZoneTurnStartedMessage(int turn)
        {
            Turn = turn;
        }
    }

    /// <summary>
    /// Команда: нанесён урон (для активации зоны)
    /// </summary>
    public readonly struct ZoneDamageDealtMessage
    {
        public readonly int Turn;

        public ZoneDamageDealtMessage(int turn)
        {
            Turn = turn;
        }
    }

    /// <summary>
    /// Команда: figure завершила ход (проверка урона зоны)
    /// </summary>
    public readonly struct ZoneFigureTurnEndedMessage
    {
        public readonly IZoneDamageTarget Target;
        public readonly int Row;
        public readonly int Col;

        public ZoneFigureTurnEndedMessage(IZoneDamageTarget target, int row, int col)
        {
            Target = target;
            Row = row;
            Col = col;
        }
    }
}
