using System;

namespace Sparcpoint.Extensions.Azure.ServiceBus.Internal
{
    internal class ServiceBusMessageDto<T>
    {
        public string MessageType { get; set; }
        public T Message { get; set; }
        public DateTimeOffset EnqueueTime { get; set; }
    }
}
