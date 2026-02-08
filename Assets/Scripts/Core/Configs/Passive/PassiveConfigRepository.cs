using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Passive
{
    [Serializable]
    public sealed class PassiveConfigRepository : ConfigRepository<PassiveConfig>
    {
        private PassiveConfig[] _passives = Array.Empty<PassiveConfig>();

        [JsonProperty("content")]
        public PassiveConfig[] Passives
        {
            get => _passives;
            set { _passives = value ?? Array.Empty<PassiveConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<PassiveConfig> Items => _passives;
        protected override string GetKey(PassiveConfig item) => item.Id;
    }
}
