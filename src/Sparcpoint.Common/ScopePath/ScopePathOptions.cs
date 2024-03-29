namespace Sparcpoint;

public static class ScopePathOptions
{
    private static char _SegementSeparator = '/';

    public static char SegmentSeparator
    {
        get => _SegementSeparator;
        set
        {
            if (!IsValidSegmentSeparator(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Segment separator must be between ASCII Code 33 (!) and 126 (~).");

            _SegementSeparator = value;
        }
    }

    public static List<char> AliasSeparators { get; } = new List<char>(new[] { '\\', ':' });

    public static bool IsValidSegmentSeparator(char separator)
    {
        return (separator >= 33 && separator <= 126);
    }
}
