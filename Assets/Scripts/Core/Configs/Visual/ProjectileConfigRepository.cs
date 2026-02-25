using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Visual
{
    [Serializable]
    public sealed class ProjectileConfigRepository : ConfigRepository<ProjectileConfig>
    {
        [JsonProperty("content")]
        public ProjectileConfig[] Configs
        {
            get => _configs;
            set { _configs = value ?? Array.Empty<ProjectileConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<ProjectileConfig> Items => _configs;
        protected override string GetKey(ProjectileConfig item) => item.Id;
        
        private ProjectileConfig[] _configs = Array.Empty<ProjectileConfig>();
    }
}
