using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint;

public static class Ensure
{
    public static void ArgumentNotNull([NotNull] object? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value == null)
            throw new ArgumentNullException(parameterName);
    }

    public static void ArgumentNotNullOrWhiteSpace([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} is null or contains only whitespace.", parameterName);
    }

    public static void NotNull([NotNull] object? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value == null)
            throw new InvalidOperationException($"{parameterName} is null.");
    }

    public static void Null([MaybeNull] object? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value != null)
            throw new InvalidOperationException($"{parameterName} is not null.");
    }

    public static void NotNullOrWhiteSpace([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{parameterName} is null or contains only whitespace.");
    }
}
