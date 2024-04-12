﻿using Sparcpoint.Extensions.Permissions;
using System.Text.Json.Serialization;

namespace Sparcpoint.Extensions.Resources;

public abstract class SparcpointResource
{
    [ResourceId, JsonIgnore]
    public ScopePath ResourceId { get; set; }
    [ResourcePermissions, JsonIgnore]
    public AccountPermissions Permissions { get; set; } = new();

    public string ResourceType => ResourceTypeAttribute.GetResourceType(this.GetType());
    public string Name => ResourceId.Segments.Last();

    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
}
