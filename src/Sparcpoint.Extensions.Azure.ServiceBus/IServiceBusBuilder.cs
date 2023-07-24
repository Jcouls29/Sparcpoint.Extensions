using Sparcpoint.Extensions.Azure.ServiceBus;
using Sparcpoint.Queues;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IServiceBusBuilder
    {
        IServiceBusBuilder Publish<T>(ServiceBusOptions options) where T : class, new();
        IServiceBusBuilder Subscribe<T>(ServiceBusOptions options, string subscriptionName, Func<IServiceProvider, ISubscriber<T>> handler);
        IServiceBusBuilderWithOptions WithOptions(ServiceBusOptions options);
    }
}
