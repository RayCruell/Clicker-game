using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Game
{
    public class EnemyTemplateConverter : JsonConverter<CEnemyTemplate>
    {
        public override CEnemyTemplate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonDoc = JsonDocument.ParseValue(ref reader);
            try
            {
                // Получаем тип объекта
                string type = jsonDoc.RootElement.GetProperty("$type").GetString();
                switch (type)
                {
                    case "CArmoredEnemyTemplate":
                        return JsonSerializer.Deserialize<CArmoredEnemyTemplate>(jsonDoc.RootElement.GetRawText(), options);

                    case "CNormalEnemyTemplate":
                        return JsonSerializer.Deserialize<CNormalEnemyTemplate>(jsonDoc.RootElement.GetRawText(), options);

                    case "CShrinkingEnemyTemplate":
                        return JsonSerializer.Deserialize<CShrinkingEnemyTemplate>(jsonDoc.RootElement.GetRawText(), options);

                    case "CHealingEnemyTemplate":
                        return JsonSerializer.Deserialize<CHealingEnemyTemplate>(jsonDoc.RootElement.GetRawText(), options);

                    default:
                        throw new NotSupportedException($"Unknown type: {type}");
                }
            }
            finally
            {
                jsonDoc.Dispose();
            }
        }

        public override void Write(Utf8JsonWriter writer, CEnemyTemplate value, JsonSerializerOptions options)
        {
            string type = value.GetType().Name;
            string json = JsonSerializer.Serialize(value, value.GetType(), options);

            var jsonDoc = JsonDocument.Parse(json);
            try
            {
                writer.WriteStartObject();
                writer.WriteString("$type", type);

                foreach (var property in jsonDoc.RootElement.EnumerateObject())
                {
                    property.WriteTo(writer);
                }

                writer.WriteEndObject();
            }
            finally
            {
                jsonDoc.Dispose();
            }
        }
    }
}