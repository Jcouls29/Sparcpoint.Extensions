using System.Diagnostics.CodeAnalysis;

namespace Sparcpoint.Extensions.Resources;

public readonly struct ResourceId : IEquatable<ResourceId>, IComparable<ResourceId>
{
    public ResourceId()
    {
        ResourceType = new ResourceType();
        Value = new ScopePath();
    }

    public ResourceId(ResourceType type, ScopePath value)
    {
        ResourceType = type;
        Value = value;
    }

    public ResourceType ResourceType { get; }
    public ScopePath Value { get; }

    public int CompareTo(ResourceId other)
    {
        return Id.CompareTo(other.Id);
    }

    public bool Equals(ResourceId other)
    {
        if (ResourceType != other.ResourceType)
            return false;

        return Value == other.Value;
    }

    public ScopePath Id => ResourceType.GetScope().Append(Value);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj == null)
            return false;

        if (obj is ResourceId resourceId)
            return (Id == resourceId.Id);

        return false;
    }

    public override int GetHashCode()
        => Id.GetHashCode();

    public override string ToString()
        => Id.ToString();

    public static bool TryParse(string value, [NotNullWhen(true)] out ResourceId? resourceId)
    {
        resourceId = null;
        if (!ScopePath.TryParse(value, out var scopePath))
            return false;

        var scope = scopePath.Value;
        if (scope.Segments.Length < 8)
            return false;

        if (!ResourceType.TryParse(scope.Segments.Take(7).ToArray(), out ResourceType? resourceType))
            return false;

        var resourceIdValue = new ScopePath(scope.Segments.Skip(7).ToArray());
        resourceId = new ResourceId(resourceType.Value, resourceIdValue);

        return true;
    }

    public static ResourceId Parse(string value)
    {
        if (!TryParse(value, out ResourceId? resourceId))
            throw new InvalidOperationException($"Could not parse ResourceId from value '{value}'.");

        return resourceId.Value;
    }

    public static implicit operator ResourceId(string value)
        => Parse(value);
    public static implicit operator string(ResourceId value)
        => value.ToString();
    public static bool operator ==(ResourceId left, ResourceId right)
        => left.Id == right.Id;
    public static bool operator !=(ResourceId left, ResourceId right)
        => left.Id != right.Id;
}
