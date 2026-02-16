namespace Project.Gameplay.Gameplay.Board.Messages
{
    public readonly struct BoardCapacityChangedMessage
    {
        public readonly int Used;
        public readonly int Capacity;

        public BoardCapacityChangedMessage(int used, int capacity)
        {
            Used = used;
            Capacity = capacity;
        }
    }
}

