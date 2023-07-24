using Microsoft.Extensions.DependencyInjection;
using Sparcpoint.Extensions.Azure.ServiceBus;
using Sparcpoint.Queues;
using System;

namespace Sparcpoint.Extensions.Azure.ServiceBus.Internal
{
    internal class DefaultServiceBusBuilderWithOptions : IServiceBusBuilderWithOptions
    {
        private readonly IServiceBusBuilder _Builder;
        private readonly ServiceBusOptions _Options;

        public DefaultServiceBusBuilderWithOptions(IServiceBusBuilder builder, ServiceBusOptions options)
        {
            _Builder = builder;
            _Options = options;
        }

        public IServiceBusBuilderWithOptions Publish<T>() where T : class, new()
        {
            _Builder.Publish<T>(_Options);
            return this;
        }

        public IServiceBusBuilderWithOptions Subscribe<T>(string subscriptionName, Func<IServiceProvider, ISubscriber<T>> handler)
        {
            _Builder.Subscribe(_Options, subscriptionName, handler);
            return this;
        }
    }
}
