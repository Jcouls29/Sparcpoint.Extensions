using System.Text.Json.Serialization;

namespace Sparcpoint.Extensions.Objects;

public abstract record SparcpointObject : ISparcpointObject
{
    [JsonConverter(typeof(ScopePathJsonConverter))]
    public ScopePath Id { get; set; } = ScopePath.RootScope;
    public string Name { get; set; } = string.Empty;

    public IReadOnlyDictionary<string, string> GetProperties()
    {
        throw new NotImplementedException();
    }

    public string? GetProperty(string key)
    {
        throw new NotImplementedException();
    }

    public void SetProperties(IReadOnlyDictionary<string, string> properties)
    {
        throw new NotImplementedException();
    }
}

// TODO: Dynamically Get / Set Properties
