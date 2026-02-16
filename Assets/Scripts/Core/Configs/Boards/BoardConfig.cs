using System;
using Newtonsoft.Json;
using Project.Core.Core.Infrastructure;

namespace Project.Core.Core.Configs.Boards
{
    [Serializable]
    public class BoardConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("width")]
        public int Width { get; set; } = 8;
        
        [JsonProperty("height")]
        public int Height { get; set; } = 8;
        
        [JsonProperty("board_data")]
        public string[] Board { get; set; }

        [JsonProperty("appear_id")]
        public string AppearStrategyId { get; set; }

        [JsonProperty("base_capacity")]
        public int BaseCapacity { get; set; } = 8;

        [JsonProperty("max_capacity")]
        public int MaxCapacity { get; set; } = 20;
        
        [JsonProperty("background_asset")]
        public string BackgroundAssetKey { get; set; }
        
        [JsonProperty("board_asset")]
        public string BoardAssetKey { get; set; }
        
        /// <summary>
        /// Parses board data from bracket notation format like "[W][B][W][B]" into 2D array.
        /// </summary>
        public string[,] GetBoard2D()
        {
            return BracketNotationParser.ParseRows(Board, Width, Height);
        }
    }
}