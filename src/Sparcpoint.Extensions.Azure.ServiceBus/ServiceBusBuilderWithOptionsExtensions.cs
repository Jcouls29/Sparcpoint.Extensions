using Sparcpoint.Queues;
using System;
using System.Threading.Tasks;
using System.Threading;
using Sparcpoint.Extensions.Azure.ServiceBus.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceBusBuilderWithOptionsExtensions
    {
        public static IServiceBusBuilderWithOptions Subscribe<T>(this IServiceBusBuilderWithOptions builder, string subscriptionName, Func<T, CancellationToken, Task> handler)
            where T : class, new()
        {
            return builder.Subscribe<T>(subscriptionName, (p) => new LambdaSubscriber<T>(handler));
        }

        public static IServiceBusBuilderWithOptions Subscribe<TMessage, THandler>(this IServiceBusBuilderWithOptions builder, string subscriptionName)
            where THandler : ISubscriber<TMessage>
        {
            return builder.Subscribe<TMessage>(subscriptionName, (p) => ActivatorUtilities.CreateInstance<THandler>(p));
        }
    }
}
