using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Artifacts
{
    /// <summary>
    /// Repository for artifact configurations. Loads from JSON.
    /// </summary>
    [Serializable]
    public sealed class ArtifactConfigRepository : ConfigRepository<ArtifactConfig>
    {
        [JsonProperty("content")]
        public ArtifactConfig[] Content
        {
            get => _items;
            set { _items = value ?? Array.Empty<ArtifactConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<ArtifactConfig>? Items => _items;
        protected override string GetKey(ArtifactConfig item) => item.Id;

        private ArtifactConfig[] _items = Array.Empty<ArtifactConfig>();

        /// <summary>
        /// Get all artifacts of a specific rarity.
        /// </summary>
        public IReadOnlyList<ArtifactConfig> GetByRarity(ArtifactRarity rarity)
        {
            var list = new List<ArtifactConfig>();
            foreach (var item in _items)
            {
                if (item.ParseRarity() == rarity)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        /// <summary>
        /// Get all artifacts with a specific trigger type.
        /// </summary>
        public IReadOnlyList<ArtifactConfig> GetByTrigger(ArtifactTrigger trigger)
        {
            var list = new List<ArtifactConfig>();
            foreach (var item in _items)
            {
                if (item.ParseTrigger() == trigger)
                {
                    list.Add(item);
                }
            }
            return list;
        }
    }
}
