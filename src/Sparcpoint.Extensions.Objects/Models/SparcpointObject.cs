using Sparcpoint.Common;
using System.Text.Json.Serialization;

namespace Sparcpoint.Extensions.Objects;

public abstract record SparcpointObject : ISparcpointObject
{
    [JsonConverter(typeof(ScopePathJsonConverter))]
    public ScopePath Id { get; set; } = ScopePath.RootScope;
    public string Name { get; set; } = string.Empty;

    public IReadOnlyDictionary<string, string?> GetProperties()
    {
        // TODO: How can we set this globally?
        var result = new Dictionary<string, string?>();
        JsonObjectMapper.Instance.Map(this, result);
        return result;
    }

    public string? GetProperty(string key)
        => GetProperties().TryGetValue(key, out var value) ? value : null;

    public void SetProperties(IReadOnlyDictionary<string, string?> properties)
    {
        JsonObjectMapper.Instance.Map(properties, this);
    }
}
