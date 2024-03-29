using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sparcpoint.Common;

public class TimeOnlyConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value == null)
            return default;

        return TimeOnly.ParseExact(value, "o");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        if (value == default)
        {
            writer.WriteNullValue();
        } else
        {
            writer.WriteStringValue(value.ToString("o"));
        }
    }
}