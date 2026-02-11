using System;

namespace Project.Gameplay.Gameplay.Save.Models
{
    /// <summary>
    /// Serializable state of a single item instance for save/load.
    /// </summary>
    [Serializable]
    public sealed class ItemState
    {
        public string Id { get; set; }
        public string ConfigId { get; set; }
        public int StackCount { get; set; }

        public ItemState() { }

        public ItemState(string id, string configId, int stackCount)
        {
            Id = id;
            ConfigId = configId;
            StackCount = stackCount;
        }
    }
}
