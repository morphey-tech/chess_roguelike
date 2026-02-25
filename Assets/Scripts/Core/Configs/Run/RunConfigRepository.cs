using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Run
{
    [Serializable]
    public class RunConfigRepository : ConfigRepository<RunConfig>
    {
        [JsonProperty("content")]
        public RunConfig[] Runs
        {
            get => _runs;
            set { _runs = value ?? Array.Empty<RunConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<RunConfig> Items => _runs;
        protected override string GetKey(RunConfig item) => item.Id;

        private RunConfig[] _runs = Array.Empty<RunConfig>();
    }
}