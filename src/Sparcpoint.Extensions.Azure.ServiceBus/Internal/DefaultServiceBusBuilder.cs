using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sparcpoint.Extensions.Azure.ServiceBus;
using Sparcpoint.Queues;
using System;

namespace Sparcpoint.Extensions.Azure.ServiceBus.Internal
{
    internal class DefaultServiceBusBuilder : IServiceBusBuilder
    {
        private readonly IServiceCollection _Services;

        public DefaultServiceBusBuilder(IServiceCollection services)
        {
            _Services = services;
        }

        public IServiceBusBuilder Publish<T>(ServiceBusOptions options) where T : class, new()
        {
            _Services.AddSingleton<IPublisher<T>>((p) => new ServiceBusPublisher<T>(Options.Create(options)));
            return this;
        }

        public IServiceBusBuilder Subscribe<T>(ServiceBusOptions options, string subscriptionName, Func<IServiceProvider, ISubscriber<T>> handler)
        {
            _Services.AddHostedService((p) => new ServiceBusMessageProcessor<T>(Options.Create(new _ServiceBusProcessorOptions
            {
                SubscriptionName = subscriptionName,
                ConnectionString = options.ConnectionString,
                ResourceId = options.ResourceId,
            }), new[] { handler(p) }));

            return this;
        }

        public IServiceBusBuilderWithOptions WithOptions(ServiceBusOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
