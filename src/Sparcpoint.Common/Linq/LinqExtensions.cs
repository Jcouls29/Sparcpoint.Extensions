﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq;

public static class LinqExtensions
{
    public static IEnumerable<CartesianProductEntry<T1, T2>> CartesianProduct<T1, T2>(this IEnumerable<T1> left, IEnumerable<T2> right)
    {
        foreach (var l in left)
        {
            foreach (var r in right)
            {
                yield return new CartesianProductEntry<T1, T2> { Left = l, Right = r };
            }
        }
    }

    public static async IAsyncEnumerable<TOutput> Select<T, TOutput>(this IAsyncEnumerable<T> values, Func<T, Task<TOutput>> selector)
    {
        await foreach(var value in values)
        {
            yield return await selector(value);
        }
    }

    public static async IAsyncEnumerable<TOutput> Select<T, TOutput>(this IAsyncEnumerable<T> values, Func<T, TOutput> selector)
    {
        await foreach (var value in values)
        {
            yield return selector(value);
        }
    }

    public static async IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> values, Func<T, bool> predicate)
    {
        await foreach (var value in values)
        {
            if (predicate(value))
                yield return value;
        }
    }

    public static async IAsyncEnumerable<T> Skip<T>(this IAsyncEnumerable<T> values, int count)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least one.");

        int skipped = 0;
        await foreach (var value in values)
        {
            if (skipped < count)
            {
                skipped++;
                continue;
            }

            yield return value;
        }
    }

    public static async IAsyncEnumerable<T> Take<T>(this IAsyncEnumerable<T> values, int count)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at leaste one.");

        int taken = 0;
        await foreach(var value in values)
        {
            if (taken >= count)
                yield break;

            taken++;
            yield return value;
        }
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> values, Func<T, bool>? predicate = null)
    {
        await foreach(var value in values)
        {
            if (predicate == null || predicate(value))
                return value;
        }

        return default;
    }

    public static async Task<T> FirstAsync<T>(this IAsyncEnumerable<T> values, Func<T, bool>? predicate = null)
    {
        await foreach(var value in values)
        {
            if (predicate == null || predicate(value))
                return value;
        }

        throw new InvalidOperationException("At least one value is required.");
    }

    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> values)
    {
        List<T> list = new();
        await foreach(var value in values)
        {
            list.Add(value);
        }

        return list;
    }

    public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> values)
    {
        List<T> list = new();
        await foreach (var value in values)
        {
            list.Add(value);
        }

        return list.ToArray();
    }
}

public struct CartesianProductEntry<T1, T2>
{
    public T1 Left { get; set; }
    public T2 Right { get; set; }
}
