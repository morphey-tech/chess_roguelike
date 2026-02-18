using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Turn
{
    /// <summary>
    /// Config for one action (replaces StepConfig at the action layer).
    /// Used by IActionBuilder to build ICombatAction. Can reference sub-actions for composite actions.
    /// </summary>
    public sealed class ActionConfig
    {
        /// <summary>Action type for builder lookup: "move", "attack", "move_then_attack", etc.</summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>Optional strategy id (e.g. movement/attack strategy).</summary>
        [JsonProperty("strategy")]
        public string Strategy { get; set; }

        /// <summary>Optional id for this action instance (defaults to Type if not set).</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>For composite actions, sub-action configs.</summary>
        [JsonProperty("sub_actions")]
        public ActionConfig[] SubActions { get; set; }
    }
}
