using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Passive
{
    [Serializable]
    public sealed class PassiveConfigRepository : ConfigRepository<PassiveConfig>
    {
        [JsonProperty("content")]
        public PassiveConfig[] Passives
        {
            get => _passives;
            set { _passives = value ?? Array.Empty<PassiveConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<PassiveConfig> Items => _passives;
        protected override string GetKey(PassiveConfig item) => item.Id;

        private PassiveConfig[] _passives = Array.Empty<PassiveConfig>();
    }
}
