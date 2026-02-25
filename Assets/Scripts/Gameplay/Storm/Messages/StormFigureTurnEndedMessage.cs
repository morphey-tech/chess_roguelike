using Project.Core.Core.Storm.Core;

namespace Project.Gameplay.ShrinkingZone.Messages
{
    /// <summary>
    /// Команда: figure завершила ход (проверка урона зоны)
    /// </summary>
    public readonly struct StormFigureTurnEndedMessage
    {
        public readonly IStormDamageTarget Target;
        public readonly int Row;
        public readonly int Col;

        public StormFigureTurnEndedMessage(IStormDamageTarget target, int row, int col)
        {
            Target = target;
            Row = row;
            Col = col;
        }
    }
}
