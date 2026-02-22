using Project.Core.Core.Grid;

namespace Project.Unity.Unity.Views.Components
{
    /// <summary>
    /// Интерфейс для получения позиции клетки из View
    /// </summary>
    public interface ICellPositionProvider
    {
        GridPosition Position { get; }
    }
}
