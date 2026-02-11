using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Economy
{
    [Serializable]
    public sealed class ItemConfigRepository : ConfigRepository<ItemConfig>
    {
        private ItemConfig[] _items = Array.Empty<ItemConfig>();

        [JsonProperty("content")]
        public ItemConfig[] Items_
        {
            get => _items;
            set { _items = value ?? Array.Empty<ItemConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<ItemConfig>? Items => _items;
        protected override string GetKey(ItemConfig item) => item.Id;
    }
}
