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

    public static void NotEqual(string expected, string actual, [CallerArgumentExpression(nameof(actual))] string? parameterName = null)
    {
        if (expected == actual)
            throw new InvalidOperationException($"{parameterName} has an invalid value: {actual}.");
    }

    public static void NotEqual<T>(T expected, T actual, [CallerArgumentExpression(nameof(actual))] string? parameterName = null)
    {
        if (expected == null && actual == null)
            return;

        if (expected == null || expected.Equals(actual))
            throw new InvalidOperationException($"{parameterName} has an invalid value.");
    }

    public static void NotValues(string[] expected, string actual, [CallerArgumentExpression(nameof(actual))] string? parameterName = null)
    {
        foreach(var value in expected)
        {
            if (value == actual)
                throw new InvalidOperationException($"{parameterName} has an invalid value: {value}.");
        }
    }

    public static void ConditionMet(bool result, [CallerArgumentExpression(nameof(result))] string? condition = null)
    {
        if (!result)
            throw new InvalidOperationException($"Condition '{condition}' was not met.");
    }

    public static void ConditionNotMet(bool result, [CallerArgumentExpression(nameof(result))] string? condition = null)
    {
        if (result)
            throw new InvalidOperationException($"Condition '{condition}' was met and should not have been.");
    }
}
