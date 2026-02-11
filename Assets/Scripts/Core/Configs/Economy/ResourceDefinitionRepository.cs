using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Economy
{
    [Serializable]
    public sealed class ResourceDefinitionRepository : ConfigRepository<ResourceDefinition>
    {
        private ResourceDefinition[] _items = Array.Empty<ResourceDefinition>();

        [JsonProperty("content")]
        public ResourceDefinition[] Items_
        {
            get => _items;
            set { _items = value ?? Array.Empty<ResourceDefinition>(); ResetIndex(); }
        }

        protected override IReadOnlyList<ResourceDefinition>? Items => _items;
        protected override string GetKey(ResourceDefinition item) => item.Id;
    }
}
