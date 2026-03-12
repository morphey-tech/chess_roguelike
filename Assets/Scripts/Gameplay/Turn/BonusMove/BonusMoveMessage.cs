using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Turn.BonusMove
{
    /// <summary>
    /// Сообщения о bonus move (старт/завершение).
    /// </summary>
    public readonly struct BonusMoveMessage
    {
        public const string STARTED = "bonusMoveStarted";
        public const string COMPLETED = "bonusMoveCompleted";

        public readonly string Type;
        public readonly Figure Actor;

        private BonusMoveMessage(string type, Figure actor)
        {
            Type = type;
            Actor = actor;
        }

        public static BonusMoveMessage Started(Figure actor)
        {
            return new BonusMoveMessage(STARTED, actor);
        }

        public static BonusMoveMessage Completed(Figure actor)
        {
            return new BonusMoveMessage(COMPLETED, actor);
        }
    }
}
