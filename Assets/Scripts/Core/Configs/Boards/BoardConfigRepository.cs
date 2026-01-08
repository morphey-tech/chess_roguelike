using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs
{
    [Serializable]
    public class BoardConfigRepository
    {
        [JsonProperty("boards")]
        public List<BoardConfig> Boards { get; set; }
    }
}