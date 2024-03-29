using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Objects;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryObjects(this IServiceCollection services)
    {
        services.AddSingleton(new ObjectEntries());
        services.AddSingleton(new LockObject());
        services.AddSingleton(typeof(IObjectStore<>), typeof(InMemoryObjectStore<>));
        services.AddSingleton(typeof(IObjectQuery<>), typeof(InMemoryObjectStore<>));
        services.AddSingleton<IObjectQuery, InMemoryObjectQuery>();
        services.AddSingleton<IObjectIdNameQuery, InMemoryObjectIdNameQuery>();

        return services;
    }
}
