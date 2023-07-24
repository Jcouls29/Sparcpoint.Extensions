using Microsoft.Extensions.Configuration;
using Sparcpoint.Extensions.Azure.ServiceBus;
using Sparcpoint.Extensions.Azure.ServiceBus.Internal;
using Sparcpoint.Queues;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceBusBuilderExtensions
    {
        public static IServiceBusBuilderWithOptions WithConfiguration(this IServiceBusBuilder builder, IConfiguration config)
        {
            return builder.WithOptions(CreateOptionsFromConfiguration(config));
        }

        public static IServiceBusBuilder Publish<T>(this IServiceBusBuilder builder, IConfiguration config) where T : class, new()
        {
            // TODO: Validate Options
            return builder.Publish<T>(CreateOptionsFromConfiguration(config));
        }

        public static IServiceBusBuilder Subscribe<T>(this IServiceBusBuilder builder, ServiceBusOptions options, string subscriptionName, Func<T, CancellationToken, Task> handler)
            where T: class, new()
        {
            return builder.Subscribe<T>(options, subscriptionName, (p) => new LambdaSubscriber<T>(handler));
        }

        public static IServiceBusBuilder Subscribe<T>(this IServiceBusBuilder builder, IConfiguration config, string subscriptionName, Func<T, CancellationToken, Task> handler)
            where T : class, new()
        {
            return builder.Subscribe<T>(CreateOptionsFromConfiguration(config), subscriptionName, (p) => new LambdaSubscriber<T>(handler));
        }

        public static IServiceBusBuilder Subscribe<T>(this IServiceBusBuilder builder, IConfiguration config, Func<T, CancellationToken, Task> handler)
            where T : class, new()
        {
            string subscriptionName = config["SubscriptionName"];
            return builder.Subscribe<T>(CreateOptionsFromConfiguration(config), subscriptionName, (p) => new LambdaSubscriber<T>(handler));
        }

        public static IServiceBusBuilder Subscribe<TMessage, THandler>(this IServiceBusBuilder builder, ServiceBusOptions options, string subscriptionName)
            where THandler : ISubscriber<TMessage>
        {
            return builder.Subscribe<TMessage>(options, subscriptionName, (p) => ActivatorUtilities.CreateInstance<THandler>(p));
        }

        public static IServiceBusBuilder Subscribe<TMessage, THandler>(this IServiceBusBuilder builder, IConfiguration config, string subscriptionName)
            where THandler : ISubscriber<TMessage>
        {
            return builder.Subscribe<TMessage>(CreateOptionsFromConfiguration(config), subscriptionName, (p) => ActivatorUtilities.CreateInstance<THandler>(p));
        }

        public static IServiceBusBuilder Subscribe<TMessage, THandler>(this IServiceBusBuilder builder, IConfiguration config)
            where THandler : ISubscriber<TMessage>
        {
            string subscriptionName = config["SubscriptionName"];
            return builder.Subscribe<TMessage>(CreateOptionsFromConfiguration(config), subscriptionName, (p) => ActivatorUtilities.CreateInstance<THandler>(p));
        }

        private static ServiceBusOptions CreateOptionsFromConfiguration(IConfiguration config)
        {
            return new ServiceBusOptions
            {
                ConnectionString = config[nameof(ServiceBusOptions.ConnectionString)],
                ResourceId = config[nameof(ServiceBusOptions.ResourceId)],
            };
        }
    }
}
