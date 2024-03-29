using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sparcpoint.Common;

public class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value == null)
            return default;

        return DateOnly.ParseExact(value, "o");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
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
