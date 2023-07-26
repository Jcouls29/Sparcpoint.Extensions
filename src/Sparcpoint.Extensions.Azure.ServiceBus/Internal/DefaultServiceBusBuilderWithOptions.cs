using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sparcpoint.Extensions.Azure.ServiceBus;
using Sparcpoint.Queues;
using System;

namespace Sparcpoint.Extensions.Azure.ServiceBus.Internal
{
    internal class DefaultServiceBusBuilderWithOptions : IServiceBusBuilderWithOptions
    {
        private readonly IServiceCollection _Services;
        private readonly ServiceBusOptions _Options;

        public DefaultServiceBusBuilderWithOptions(IServiceCollection services, ServiceBusOptions options)
        {
            _Services = services;
            _Options = options;
        }

        public IServiceBusBuilderWithOptions Publish<T>() where T : class, new()
        {
            _Services.AddSingleton<IPublisher<T>>((p) => new ServiceBusPublisher<T>(Options.Create(_Options)));
            return this;
        }

        public IServiceBusBuilderWithOptions Subscribe<T>(string subscriptionName, Func<IServiceProvider, ISubscriber<T>> handler)
        {
            _Services.AddHostedService((p) => new ServiceBusMessageProcessor<T>(Options.Create(new _ServiceBusProcessorOptions
            {
                SubscriptionName = subscriptionName,
                ConnectionString = _Options.ConnectionString,
                ResourceId = _Options.ResourceId,
            }), new[] { handler(p) }));

            return this;
        }
    }

    internal class CreateTopicServiceBusBuilderWithOptions : IServiceBusBuilderWithOptions
    {
        private readonly IServiceCollection _Services;
        private readonly ServiceBusOptions _Options;

        public CreateTopicServiceBusBuilderWithOptions(IServiceCollection services, ServiceBusOptions options)
        {
            _Services = services;
            _Options = options;
        }

        public IServiceBusBuilderWithOptions Publish<T>() where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IServiceBusBuilderWithOptions Subscribe<T>(string subscriptionName, Func<IServiceProvider, ISubscriber<T>> handler)
        {
            throw new NotImplementedException();
        }
    }
}
