using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Project.Core.Core.Infrastructure
{
    public class Vec2ObservableDictionaryConverter : JsonConverter<Dictionary<Vector2Int, string>> 
    {
        public override void WriteJson(JsonWriter writer, Dictionary<Vector2Int, string> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                string key = $"{kvp.Key.x},{kvp.Key.y}";
                writer.WritePropertyName(key);
                serializer.Serialize(writer, kvp.Value);
            }

            writer.WriteEndObject();
        }

        public override Dictionary<Vector2Int, string> ReadJson(JsonReader reader, Type objectType, Dictionary<Vector2Int, string> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var dictionary = new Dictionary<Vector2Int, string>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string key = reader.Value.ToString();
                    var parts = key.Split(',');

                    if (parts.Length != 2)
                    {
                        throw new JsonSerializationException("Invalid format for Vector2Int key.");
                    }

                    int x = int.Parse(parts[0]);
                    int y = int.Parse(parts[1]);
                    Vector2Int vectorKey = new(x, y);

                    reader.Read();
                    string value = serializer.Deserialize<string>(reader);
                    dictionary[vectorKey] = value;
                }
            }

            return dictionary;
        }
    }
}