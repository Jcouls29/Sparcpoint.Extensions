namespace Sparcpoint.Extensions.Azure;

public static class TableStorageFilter
{
    public static string CreatePrefixRangeFilter(string key, string value)
    {
        string upperLimit = GetPrefixUpperLimitValue(value);
        return $"({key} ge '{value}' and {key} lt '{upperLimit}')";
    }

    public static string GetPrefixUpperLimitValue(string value)
    {
        char[] prefix = value.ToCharArray();
        if (prefix[prefix.Length - 1] == char.MaxValue)
            throw new InvalidOperationException("An upper limit for the prefix filter cannot be created because the last character of the row key prefix is \\uFFFF which is invalid.");

        prefix[prefix.Length - 1]++;

        return new string(prefix);
    }
}
