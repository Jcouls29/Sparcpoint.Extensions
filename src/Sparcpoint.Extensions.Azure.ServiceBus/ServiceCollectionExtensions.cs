using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Sparcpoint.Abstractions;
using Sparcpoint.Extensions.Azure.ServiceBus.Internal;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceBusBuilder AddServiceBusMessaging(this ISparcpointServiceCollection services)
        {
            return new DefaultServiceBusBuilder(services.Services);
        }
    }
}
