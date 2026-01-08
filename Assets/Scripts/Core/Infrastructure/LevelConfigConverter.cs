using System;
using Newtonsoft.Json;
using Project.Core.Core.Configs;

namespace Project.Core.Core.Infrastructure
{
     public class LevelConfigConverter : JsonConverter<LevelConfig>
    {
        public override void WriteJson(JsonWriter writer, LevelConfig value, JsonSerializer serializer)
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

        public override LevelConfig ReadJson(JsonReader reader, Type objectType, LevelConfig existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            LevelConfig levelConfig = new();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string? propertyName = reader.Value.ToString();
                    switch (propertyName)
                    {
                        case "Width":
                            reader.Read();
                            levelConfig.Width = (int)reader.Value;
                            break;
                        case "Height":
                            reader.Read();
                            levelConfig.Height = (int)reader.Value;
                            break;
                        case "Board":
                            reader.Read();
                            string[]? board = serializer.Deserialize<string[]>(reader);
                            levelConfig.Board = board;
                            break;
                    }
                }
            }
            return levelConfig;
        }
    }
}