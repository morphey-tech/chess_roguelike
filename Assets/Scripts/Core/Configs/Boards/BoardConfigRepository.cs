using System;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Boards
{
    [Serializable]
    public class BoardConfigRepository
    {
        [JsonProperty("content")]
        public BoardConfig[] Boards { get; set; }

        public BoardConfig? GetBy(string id)
        {
            foreach (BoardConfig config in Boards)
            {
                if (config.Id != id)
                {
                    continue;
                }
                return config;
            }
            return null;
        }
    }
}