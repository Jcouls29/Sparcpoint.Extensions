using System;

namespace Sparcpoint.Queues
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class QueueNameAttribute : Attribute
    {
        public QueueNameAttribute(string queueName)
        {
            QueueName = queueName;
        }

        public string QueueName { get; }
    }
}
