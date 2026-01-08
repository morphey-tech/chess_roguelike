using System;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Infrastructure
{
     public class BoardConfigConverter : JsonConverter<BoardConfig>
    {
        public override void WriteJson(JsonWriter writer, BoardConfig value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Width");
            writer.WriteValue(value.Width);

            writer.WritePropertyName("Height");
            writer.WriteValue(value.Height);

            writer.WritePropertyName("Board");
            writer.WriteStartArray();
            for (int i = 0; i < value.Board.Length; i++)
            {
                writer.WriteValue(value.Board[i]);
            }
            writer.WriteEndArray();
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
                    switch (propertyName)
                    {
                        case "Width":
                            reader.Read();
                            boardConfig.Width = (int)reader.Value;
                            break;
                        case "Height":
                            reader.Read();
                            boardConfig.Height = (int)reader.Value;
                            break;
                        case "Board":
                            reader.Read();
                            string[]? board = serializer.Deserialize<string[]>(reader);
                            boardConfig.Board = board;
                            break;
                    }
                }
            }
            return boardConfig;
        }
    }
}