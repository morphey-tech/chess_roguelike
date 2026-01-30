using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    public sealed class ConditionConfigRepository
    {
        [JsonProperty("content")]
        public ConditionConfig[] Conditions { get; set; }
    }
}