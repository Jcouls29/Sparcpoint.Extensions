using System.Diagnostics.CodeAnalysis;

namespace Sparcpoint.Extensions.Permissions;

public readonly record struct ScopePath
{
    public string[] Segments { get; }
    public bool IsRootScope => (this == RootScope);

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

    public override string ToString()
    {
        return "/" + string.Join("/", Segments ?? Array.Empty<string>());
    }

    public static bool TryParse(string path, [NotNullWhen(true)] out ScopePath? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(path))
        {
            result = ScopePath.RootScope;
            return true;
        }

        var normalizePath = path.Replace("\\", "/").ToLower();
        var split = normalizePath.Split('/');

        result = new ScopePath(split);
        return true;
    }

    public static explicit operator ScopePath(string path)
    {
        if (ScopePath.TryParse(path, out var result))
            return result.Value;

        throw new InvalidOperationException($"Could not parse the result.");
    }

    public static implicit operator string(ScopePath scope)
    {
        return scope.ToString();
    }

    public static ScopePath RootScope { get; } = new ScopePath();
}
