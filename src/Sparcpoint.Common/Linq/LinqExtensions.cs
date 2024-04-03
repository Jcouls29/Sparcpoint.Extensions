using System;
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
