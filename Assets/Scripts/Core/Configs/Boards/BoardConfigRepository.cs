using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project.Core.Core.Configs.Boards
{
    [Serializable]
    public class BoardConfigRepository : ConfigRepository<BoardConfig>
    {
        [JsonProperty("content")]
        public BoardConfig[] Boards
        {
            get => _boards;
            set { _boards = value ?? Array.Empty<BoardConfig>(); ResetIndex(); }
        }

        protected override IReadOnlyList<BoardConfig> Items => _boards;
        protected override string GetKey(BoardConfig item) => item.Id;
        
        private BoardConfig[] _boards = Array.Empty<BoardConfig>();
    }
}