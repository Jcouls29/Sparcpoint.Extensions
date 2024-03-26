using System.Diagnostics.CodeAnalysis;

namespace Sparcpoint.Extensions.Permissions;

public readonly struct ScopePath
{
    public string[] Segments { get; }
    public bool IsRootScope => (this == RootScope);
    public int Rank => Segments.Length;

    public ScopePath()
    {
        Segments = Array.Empty<string>();
    }

    public ScopePath(string[]? segments)
    {
        Segments = segments ?? Array.Empty<string>();
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

    public ScopePath Append(ScopePath other)
    {
        var finalSegments = Array.Empty<string>();
        if (Segments.Length > 0)
        {
            finalSegments = Segments;
        }

        if (other.Segments.Length > 0)
        {
            finalSegments = finalSegments.Concat(other.Segments).ToArray();
        }

        return new ScopePath(finalSegments);
    }

    public ScopePath Back(int numberLevels = 1)
    {
        if (numberLevels < 0)
            throw new ArgumentOutOfRangeException(nameof(numberLevels), "Number of levels to go back cannot be less than 0.");

        if (numberLevels == 0)
            return this;

        if (Segments.Length <= numberLevels)
            return ScopePath.RootScope;

        return new ScopePath(Segments.Take(Segments.Length - numberLevels).ToArray());
    }

    public override int GetHashCode()
    {
        int code = 0;

        foreach(var s in Segments)
            code = HashCode.Combine(code, s.GetHashCode());

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
        return SequenceStartsWith(right.Back(), left);
    }

    public static bool operator >(ScopePath left, ScopePath right)
    {
        return SequenceStartsWith(left.Back(), right);
    }

    public static bool operator <=(ScopePath left, ScopePath right)
    {
        return SequenceStartsWith(right, left);
    }

    public static bool operator >=(ScopePath left, ScopePath right)
    {
        return SequenceStartsWith(left, right);
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

    private static bool SequenceStartsWith(ScopePath sequence, ScopePath startsWith)
    {
        var itSequence = sequence.Segments.GetEnumerator();
        var itStarts = startsWith.Segments.GetEnumerator();

        while(itStarts.MoveNext())
        {
            var el = itStarts.Current;

            if (!itSequence.MoveNext())
                return false;

            if (!el.Equals(itSequence.Current))
                return false;
        }

        return true;
    }
}
