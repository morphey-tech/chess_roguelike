using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.ShrinkingZone.Config
{
    /// <summary>
    /// Репозиторий конфигураций shrinking zone
    /// </summary>
    [Serializable]
    public class ZoneShrinkConfigRepository : ConfigRepository<ZoneShrinkConfig>
    {
        private ZoneShrinkConfig[] _content = Array.Empty<ZoneShrinkConfig>();

        [JsonProperty("content")]
        public ZoneShrinkConfig[] Content
        {
            get => _content;
            set { _content = value ?? Array.Empty<ZoneShrinkConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<ZoneShrinkConfig> Items => _content;
        protected override string GetKey(ZoneShrinkConfig item) => item.Id;
    }
}
