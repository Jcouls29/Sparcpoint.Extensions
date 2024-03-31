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
}
