using System.Diagnostics.CodeAnalysis;

namespace Sparcpoint;

public readonly struct ScopePath
{
    private readonly string[] _Segments = Array.Empty<string>();
    public string[] Segments
    {
        get => _Segments ?? Array.Empty<string>();
    }
    public bool IsRootScope => (this == RootScope);
    public int Rank => Segments.Length;

    public ScopePath()
    {
        _Segments = Array.Empty<string>();
    }

    public ScopePath(string[]? segments)
    {
        _Segments = segments ?? Array.Empty<string>();
        ValidateSegments();
    }

    private void ValidateSegments()
    {
        if (Segments.Any(s => string.IsNullOrWhiteSpace(s)))
            throw new InvalidOperationException("Segments cannot be null or only contain whitespace.");

        // TODO: Allow for different separators through some form of configuration
        if (Segments.Any(s => s.Contains("/") || s.Contains("\\")))
            throw new InvalidOperationException("Segments cannot contain either a '\\' character or a '/' character.");
    }

    public override int GetHashCode()
    {
        int code = 0;

        if (Segments != null)
        {
            foreach (var s in Segments)
                code = HashCode.Combine(code, s.GetHashCode());
        }

        return code;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj == null)
            return false;

        if (obj is ScopePath otherPath)
        {
            return this.Segments.SequenceEqual(otherPath.Segments);
        }

        return false;
    }

    public override string ToString()
    {
        return "/" + string.Join("/", Segments ?? Array.Empty<string>());
    }

    public static ScopePath Parse(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return ScopePath.RootScope;

        if (!TryParse(path, out ScopePath? result))
            throw new InvalidOperationException($"Could not parse ScopePath from value '{path}'");

        return result.Value;
    }

    public static bool TryParse(string path, [NotNullWhen(true)] out ScopePath? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(path))
        {
            result = ScopePath.RootScope;
            return true;
        }

        var normalizePath = path;

        foreach(var alias in ScopePathOptions.AliasSeparators)
        {
            normalizePath = normalizePath.Replace(alias, ScopePathOptions.SegmentSeparator);
        }

        normalizePath = normalizePath.ToLower();

        var split = normalizePath.Split(ScopePathOptions.SegmentSeparator).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        result = new ScopePath(split);
        return true;
    }

    public static implicit operator ScopePath(string path)
    {
        return Parse(path);
    }

    public static implicit operator string(ScopePath scope)
    {
        return scope.ToString();
    }

    public static bool operator <(ScopePath left, ScopePath right) 
    {
        return right.Back().StartsWith(left);
    }

    public static bool operator >(ScopePath left, ScopePath right)
    {
        return left.Back().StartsWith(right);
    }

    public static bool operator <=(ScopePath left, ScopePath right)
    {
        return right.StartsWith(left);
    }

    public static bool operator >=(ScopePath left, ScopePath right)
    {
        return left.StartsWith(right);
    }

    public static ScopePath operator &(ScopePath left, ScopePath right)
    {
        return left.Append(right);
    }

    public static bool operator ==(ScopePath left, ScopePath right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ScopePath left, ScopePath right)
    {
        return !left.Equals(right);
    }

    public static ScopePath RootScope { get; } = new ScopePath();
}
