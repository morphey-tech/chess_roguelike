using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Storm
{
    [Serializable]
    public class StormConfigRepository : ConfigRepository<StormConfig>
    {
        [JsonProperty("content")]
        public StormConfig[] Content
        {
            get => _content;
            set { _content = value ?? Array.Empty<StormConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<StormConfig> Items => _content;
        protected override string GetKey(StormConfig item) => item.Id;

        private StormConfig[] _content = Array.Empty<StormConfig>();
    }
}
