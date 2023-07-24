using System.Collections.Generic;
using System;

namespace Sparcpoint.Queues
{
    public class QueueMessage<T>
    {
        public string QueueName { get; set; }
        public T Message { get; set; }
        public DateTimeOffset EnqueueTime { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
