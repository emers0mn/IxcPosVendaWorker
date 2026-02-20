using System.Text.Json;
using System.Text.Json.Serialization;

namespace IxcPosVendaWorker.Helpers;

public class StringOrIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Se vier como string "123"
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            return int.TryParse(stringValue, out var result) ? result : 0;
        }
        // Se vier como número 123
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }
        
        return 0;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}