using System.Text.Json.Serialization;

namespace Sparcpoint.Extensions.Objects;

public interface ISparcpointObject
{
    [JsonConverter(typeof(ScopePathJsonConverter))]
    ScopePath Id { get; }
    string Name { get; set; }

    IReadOnlyDictionary<string, string?> GetProperties();
    string? GetProperty(string key);
    void SetProperties(IReadOnlyDictionary<string, string?> properties);
}
