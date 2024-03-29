using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sparcpoint;

public class ScopePathJsonConverter : JsonConverter<ScopePath>
{
    public override ScopePath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var input = reader.GetString();
        if (string.IsNullOrEmpty(input))
            return ScopePath.RootScope;

        return ScopePath.Parse(input);
    }

    public override void Write(Utf8JsonWriter writer, ScopePath value, JsonSerializerOptions options)
    {
        if (value == ScopePath.RootScope)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.ToString());
    }
}