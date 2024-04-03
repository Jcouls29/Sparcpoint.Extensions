using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Objects;

public static class ObjectQueryExtensions
{
    public static async Task<T?> FirstOrDefaultAsync<T>(this IObjectQuery<T> query, ObjectQueryParameters parameters) where T : class, ISparcpointObject
    {
        await foreach(var item in query.RunAsync(parameters)) 
        {
            return item;
        }

        return null;
    }

    public static async Task<List<T>> ToListAsync<T>(this IObjectQuery<T> query, ObjectQueryParameters parameters) where T : class, ISparcpointObject
    {
        List<T> results = new();
        await foreach (var item in query.RunAsync(parameters))
            results.Add(item);

        return results;
    }

    public static async Task<T[]> ToArrayAsync<T>(this IObjectQuery<T> query, ObjectQueryParameters parameters) where T : class, ISparcpointObject
    {
        List<T> results = new();
        await foreach (var item in query.RunAsync(parameters))
            results.Add(item);

        return results.ToArray();
    }
}
