using Sparcpoint.Extensions.Resources;
using Sparcpoint.Extensions.Resources.InMemory;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryResources(this IServiceCollection services)
    {
        var collection = new InMemoryResourceCollection();
        var store = new InMemoryResourceStore(collection);

        return services.AddSingleton<ISparcpointClientFactory>(new SparcpointClientFactory(store));
    }
}
