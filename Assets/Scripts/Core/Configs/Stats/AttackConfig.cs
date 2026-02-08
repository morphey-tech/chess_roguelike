using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stats
{
    [Serializable]
    public sealed class AttackConfig
    {
        [JsonProperty("type")]
        public AttackType Type { get; set; }

        [JsonProperty("damage")]
        public int Damage { get; set; }

        [JsonProperty("range")]
        public int Range { get; set; }

        [JsonProperty("targeting")]
        public TargetingType Targeting { get; set; }

        [JsonProperty("delivery")]
        public DeliveryType Delivery { get; set; }

        [JsonProperty("pattern")]
        public HitPattern Pattern { get; set; }

        [JsonProperty("projectile_id")]
        public string ProjectileId { get; set; }
    }
}
