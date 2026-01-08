using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Player
{
    [Serializable]
    public class PlayerConfig
    {
        [JsonProperty("max_squad_size")]
        public int MaxSquadSize { get; set; } = 8;
    }
}