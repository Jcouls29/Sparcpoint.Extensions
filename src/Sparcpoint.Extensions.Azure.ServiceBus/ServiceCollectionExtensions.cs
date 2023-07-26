using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Sparcpoint.Abstractions;
using Sparcpoint.Extensions.Azure.ServiceBus;
using Sparcpoint.Extensions.Azure.ServiceBus.Internal;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceBusBuilderWithOptions AddServiceBusMessaging(this ISparcpointServiceCollection services, ServiceBusOptions options)
        {
            if (options.CreateTopicIfNotExists)
            {
                
            } else
            {
                return new DefaultServiceBusBuilderWithOptions(services.Services, options);
            }
        }

        public static IServiceBusBuilderWithOptions AddServiceBusMessaging(this ISparcpointServiceCollection services, IConfiguration config)
        {
            return services.AddServiceBusMessaging(CreateOptionsFromConfiguration(config));
        }

        private static ServiceBusOptions CreateOptionsFromConfiguration(IConfiguration config)
        {
            return new ServiceBusOptions
            {
                ConnectionString = config[nameof(ServiceBusOptions.ConnectionString)],
                CreateTopicIfNotExists = bool.TryParse(config[nameof(ServiceBusOptions.CreateTopicIfNotExists)], out bool createTopic) ? createTopic : false,
                ResourceId = config[nameof(ServiceBusOptions.ResourceId)],
            };
        }
    }
}
