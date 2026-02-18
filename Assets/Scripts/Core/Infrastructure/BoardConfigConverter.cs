using System;
using Newtonsoft.Json;
using Project.Core.Core.Configs;
using Project.Core.Core.Configs.Boards;

namespace Project.Core.Core.Infrastructure
{
     public class BoardConfigConverter : JsonConverter<BoardConfig>
    {
        public override void WriteJson(JsonWriter writer, BoardConfig value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Id");
            writer.WriteValue(value.Id);

            writer.WritePropertyName("Width");
            writer.WriteValue(value.Width);

            writer.WritePropertyName("Height");
            writer.WriteValue(value.Height);

            writer.WritePropertyName("Board");
            writer.WriteStartArray();
            for (int i = 0; i < value.Cells.Length; i++)
            {
                writer.WriteValue(value.Cells[i]);
            }
            writer.WriteEndArray();

            writer.WritePropertyName("AppearId");
            writer.WriteValue(value.CellsAppearStrategyId);
            writer.WriteEndObject();
        }

        public override BoardConfig ReadJson(JsonReader reader, Type objectType, BoardConfig existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            BoardConfig boardConfig = new();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string? propertyName = reader.Value.ToString();
                    string prop = propertyName?.Trim().ToLowerInvariant() ?? string.Empty;
                    switch (prop)
                    {
                        case "id":
                            reader.Read();
                            boardConfig.Id = reader.Value?.ToString();
                            break;
                        case "width":
                            reader.Read();
                            boardConfig.Width = (int)reader.Value;
                            break;
                        case "height":
                            reader.Read();
                            boardConfig.Height = (int)reader.Value;
                            break;
                        case "board":
                        case "board_data":
                            reader.Read();
                            string[]? board = serializer.Deserialize<string[]>(reader);
                            boardConfig.Cells = board;
                            break;
                        case "appear_id":
                        case "appearid":
                        case "appearstrategyid":
                            reader.Read();
                            boardConfig.CellsAppearStrategyId = reader.Value?.ToString();
                            break;
                    }
                }
            }
            return boardConfig;
        }
    }
}