using Sparcpoint.Abstractions;
using Sparcpoint.Abstractions.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static ISparcpointServiceCollection AddSparcpointServices(this IServiceCollection services)
        {
            // FUTURE: Add default sparcpoint services
            return new DefaultSparcpointServiceCollection(services);
        }
    }

}
