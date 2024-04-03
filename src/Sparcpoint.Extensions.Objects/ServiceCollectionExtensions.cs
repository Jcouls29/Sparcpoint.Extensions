using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        services.TryAddSingleton(new ObjectEntries());
        services.TryAddSingleton(new LockObject());
        services.TryAddSingleton(typeof(IObjectStore<>), typeof(InMemoryObjectStore<>));
        services.TryAddSingleton(typeof(IObjectQuery<>), typeof(InMemoryObjectStore<>));
        services.TryAddSingleton<IObjectQuery, InMemoryObjectQuery>();
        services.TryAddSingleton<IObjectIdNameQuery, InMemoryObjectIdNameQuery>();
        services.TryAddSingleton<IObjectStoreFactory, InMemoryObjectStoreFactory>();
        
        return services;
    }
}
