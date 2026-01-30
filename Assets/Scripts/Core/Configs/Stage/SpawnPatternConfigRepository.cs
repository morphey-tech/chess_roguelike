using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Stage
{
    public class SpawnPatternConfigRepository
    {
        [JsonProperty("content")]
        public SpawnPatternConfig[] Patterns { get; set; }
    }
}
