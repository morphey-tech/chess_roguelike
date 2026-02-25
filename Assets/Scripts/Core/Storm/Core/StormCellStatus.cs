namespace Project.Core.Core.Storm.Core
{
    public enum StormCellStatus
    {
        Safe = 0,      // Безопасная клетка
        Warning = 1,   // Предупреждение (скоро станет опасной)
        Danger = 2     // Опасная клетка (наносит урон)
    }
}