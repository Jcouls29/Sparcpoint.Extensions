using System.Threading;
using System.Threading.Tasks;

namespace Sparcpoint.Queues
{
    public abstract class Subscriber<T> : ISubscriber<T>
    {
        protected abstract Task OnMessageReceivedAsync(T message, CancellationToken cancellationToken = default);

        public Task OnMessageReceivedAsync(QueueMessage<T> message, CancellationToken cancellationToken = default)
        {
            return OnMessageReceivedAsync(message.Message, cancellationToken);
        }
    }
}
