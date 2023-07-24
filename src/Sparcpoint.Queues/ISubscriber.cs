using System.Threading;
using System.Threading.Tasks;

namespace Sparcpoint.Queues
{
    public interface ISubscriber<T>
    {
        Task OnMessageReceivedAsync(QueueMessage<T> message, CancellationToken cancellationToken = default);
    }
}
