using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Economy
{
    [Serializable]
    public sealed class ItemConfigRepository : ConfigRepository<ItemConfig>
    {
        [JsonProperty("content")]
        public ItemConfig[] Content
        {
            get => _items;
            set { _items = value ?? Array.Empty<ItemConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<ItemConfig>? Items => _items;
        protected override string GetKey(ItemConfig item) => item.Id;

        private ItemConfig[] _items = Array.Empty<ItemConfig>();
    }
}
