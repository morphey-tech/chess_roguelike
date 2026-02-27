using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Passive
{
    /// <summary>
    /// Репозиторий для хранения и доступа к конфигам пассивок.
    /// </summary>
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

        /// <summary>
        /// Возвращает все конфиги пассивок.
        /// </summary>
        public IReadOnlyList<PassiveConfig> GetAll() => _passives;
    }
}
