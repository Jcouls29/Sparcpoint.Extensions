using Sparcpoint.Queues;
using System;
using Microsoft.Extensions.Options;
using System.Reflection;
using Sparcpoint.Extensions.Azure.ServiceBus;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IServiceBusBuilderWithOptions
    {
        IServiceBusBuilderWithOptions Publish<T>() where T : class, new();
        IServiceBusBuilderWithOptions Subscribe<T>(string subscriptionName, Func<IServiceProvider, ISubscriber<T>> handler);
    }
}
