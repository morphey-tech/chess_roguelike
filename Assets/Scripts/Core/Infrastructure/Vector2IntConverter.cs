using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Project.Core.Core.Infrastructure
{
    public class Vector2IntConverter : JsonConverter<Vector2Int>
    {
        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value.x},{value.y}");
        }

        public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            string str = reader.Value.ToString();
            string[]? parts = str.Split(',');

            if (parts.Length != 2)
            {
                throw new JsonSerializationException("Invalid format for Vector2Int.");
            }

            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            return new Vector2Int(x, y);
        }
    }
}