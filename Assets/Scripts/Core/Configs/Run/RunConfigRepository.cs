using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Configs.Run
{
    [Serializable]
    public class RunConfigRepository : ConfigRepository<RunConfig>
    {
        private RunConfig[] _runs = Array.Empty<RunConfig>();

        [JsonProperty("content")]
        public RunConfig[] Runs
        {
            get => _runs;
            set { _runs = value ?? Array.Empty<RunConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<RunConfig> Items => _runs;
        protected override string GetKey(RunConfig item) => item.Id;
    }
}