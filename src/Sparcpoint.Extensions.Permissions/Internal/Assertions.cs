using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sparcpoint.Extensions.Permissions;

internal static class Assertions
{
    public static void NotEmptyOrWhitespace([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? parameter = default)
    {
        if (value == null)
            throw new ArgumentNullException(parameter);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{parameter}' cannot be empty or contain only whitespace.");
    }
}