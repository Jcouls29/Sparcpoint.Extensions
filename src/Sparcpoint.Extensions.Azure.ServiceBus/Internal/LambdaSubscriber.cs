using Sparcpoint.Queues;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Azure.ServiceBus.Internal
{
    internal class LambdaSubscriber<T> : ISubscriber<T>
        where T : class, new()
    {
        private readonly Func<T, CancellationToken, Task> _Handler;

        public LambdaSubscriber(Func<T, CancellationToken, Task> handler)
        {
            _Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task OnMessageReceivedAsync(QueueMessage<T> message, CancellationToken cancellationToken = default)
        {
            return _Handler(message.Message, cancellationToken);
        }
    }
}
