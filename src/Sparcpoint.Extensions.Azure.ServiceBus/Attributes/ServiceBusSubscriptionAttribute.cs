using System;

namespace Sparcpoint.Extensions.Azure.ServiceBus.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ServiceBusSubscriptionAttribute : Attribute
    {
        public ServiceBusSubscriptionAttribute(string topicName, string subscriptionName)
        {
            TopicName = topicName;
            SubscriptionName = subscriptionName;
        }

        public string TopicName { get; }
        public string SubscriptionName { get; }
    }
}
