using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Loot;

namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public sealed class LootVisualContext
    {
        public GridPosition DropPosition { get; }
        public LootResult Loot { get; }

        public LootVisualContext(GridPosition dropPosition, LootResult loot)
        {
            DropPosition = dropPosition;
            Loot = loot;
        }
    }
}
