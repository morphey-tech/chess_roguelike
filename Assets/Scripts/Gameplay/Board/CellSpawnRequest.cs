using Project.Core.Core.Grid;
using Project.Gameplay;

namespace Project.Gameplay.Gameplay.Board
{
    /// <summary>
    /// Один запрос на создание клетки для батчевого спавна доски.
    /// </summary>
    public readonly struct CellSpawnRequest
    {
        public Entity Entity { get; }
        public GridPosition Position { get; }
        public string SkinId { get; }

        public CellSpawnRequest(Entity entity, GridPosition position, string skinId)
        {
            Entity = entity;
            Position = position;
            SkinId = skinId;
        }
    }
}
