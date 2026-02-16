namespace Project.Gameplay.Gameplay.Board.Capacity
{
    public sealed class BoardCapacityModel
    {
        public int Capacity { get; private set; }
        public int Used { get; private set; }
        public int MaxCapacity { get; private set; }

        public int Free => Capacity - Used;

        public void Configure(int capacity, int maxCapacity)
        {
            MaxCapacity = maxCapacity < 0 ? 0 : maxCapacity;
            Capacity = Clamp(capacity, 0, MaxCapacity);
            Used = Clamp(Used, 0, Capacity);
        }

        public void SetCapacity(int value)
        {
            Capacity = Clamp(value, 0, MaxCapacity);
            Used = Clamp(Used, 0, Capacity);
        }

        public void SetUsed(int value)
        {
            Used = Clamp(value, 0, Capacity);
        }

        public bool CanReserve(int load)
        {
            int safeLoad = load < 0 ? 0 : load;
            return Used + safeLoad <= Capacity;
        }

        public bool TryReserve(int load)
        {
            int safeLoad = load < 0 ? 0 : load;
            if (Used + safeLoad > Capacity)
                return false;

            Used += safeLoad;
            return true;
        }

        public void Release(int load)
        {
            int safeLoad = load < 0 ? 0 : load;
            Used -= safeLoad;
            if (Used < 0)
                Used = 0;
        }

        public void Reset()
        {
            Used = 0;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}

