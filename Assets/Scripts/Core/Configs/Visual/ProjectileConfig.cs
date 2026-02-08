using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Visual
{
    [Serializable]
    public sealed class ProjectileConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("asset_key")]
        public string AssetKey { get; set; }

        [JsonProperty("speed")]
        public float Speed { get; set; } = 8f;

        [JsonProperty("acceleration")]
        public float Acceleration { get; set; }

        [JsonProperty("curve")]
        public string Curve { get; set; }

        [JsonProperty("homing")]
        public bool Homing { get; set; }

        [JsonProperty("turn_speed")]
        public float TurnSpeed { get; set; }

        [JsonProperty("height_offset")]
        public float HeightOffset { get; set; } = 0.1f;

        [JsonProperty("impact_fx_key")]
        public string ImpactFxKey { get; set; }

        [JsonProperty("trail_fx_key")]
        public string TrailFxKey { get; set; }

        [JsonProperty("shake")]
        public float Shake { get; set; }

        [JsonProperty("camera_zoom")]
        public float CameraZoom { get; set; }

        [JsonProperty("sound_shoot")]
        public string SoundShoot { get; set; }

        [JsonProperty("sound_hit")]
        public string SoundHit { get; set; }
    }
}
