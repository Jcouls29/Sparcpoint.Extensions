using System.Threading;
using System.Threading.Tasks;

namespace Sparcpoint.Queues
{
    public interface IPublisher<T>
    {
        Task PublishAsync(T message, CancellationToken cancellationToken = default);
    }
}
