using Sparcpoint.Extensions.Azure.Objects.BlobStorage;

namespace Sparcpoint.Extensions.IntegrationTests;

[SparcpointObject("Basic")]
public record BasicObject : SparcpointObject
{
    public string? SearchableString { get; set; } = string.Empty;
}
