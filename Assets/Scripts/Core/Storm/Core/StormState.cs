namespace Project.Core.Core.Storm.Core
{
    public enum StormState
    {
        Inactive,      // Зона ещё не активирована
        Active,        // Зона активна
        MinSizeReached // Достигнут минимальный размер
    }
}