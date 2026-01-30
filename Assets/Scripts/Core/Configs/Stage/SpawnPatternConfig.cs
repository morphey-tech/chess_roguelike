using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    [Serializable]
    public class SpawnPatternConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Pattern rows where each char is alias for figure.
        /// '.' = empty, other chars = figure alias.
        /// </summary>
        [JsonProperty("rows")]
        public string[] Rows { get; set; } = Array.Empty<string>();
    }
}
