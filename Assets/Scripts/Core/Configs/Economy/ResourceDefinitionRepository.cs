using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Economy
{
    [Serializable]
    public sealed class ResourceDefinitionRepository : ConfigRepository<ResourceDefinition>
    {
        [JsonProperty("content")]
        public ResourceDefinition[] Definitions
        {
            get => _items;
            set { _items = value ?? Array.Empty<ResourceDefinition>(); ResetIndex(); }
        }

        protected override IReadOnlyList<ResourceDefinition>? Items => _items;
        protected override string GetKey(ResourceDefinition item) => item.Id;
        
        private ResourceDefinition[] _items = Array.Empty<ResourceDefinition>();
    }
}
