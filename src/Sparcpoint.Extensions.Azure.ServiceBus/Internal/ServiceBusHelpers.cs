using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.ServiceBus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Sparcpoint.Extensions.Azure.ServiceBus.Attributes;
using Azure.Messaging.ServiceBus;
using Azure;
using Sparcpoint.Queues;

namespace Sparcpoint.Extensions.Azure.ServiceBus.Internal
{
    internal static class ServiceBusHelpers
    {
        internal static List<string> _AvailableTopics = new List<string>();
        private static object _TopicCreateLock = new object();

        // NOTE: Topics are considered case-insensitive, thuse the Ignore Case Comparer
        internal static ConcurrentDictionary<string, List<string>> _AvailableSubscriptionsPerTopic = new ConcurrentDictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private static object _SubscriptionCreateLock = new object();

        public static void CreateTopicIfNotExists(this ArmClient _ManagerClient, string topicName, string resourceId)
        {
            // NOTE: Topics are case-insensitive
            if (_AvailableTopics.Contains(topicName, StringComparer.OrdinalIgnoreCase))
                return;

            lock (_TopicCreateLock)
            {
                if (_AvailableTopics.Contains(topicName, StringComparer.OrdinalIgnoreCase))
                    return;

                ServiceBusNamespaceResource r = _ManagerClient.GetServiceBusNamespaceResource(new ResourceIdentifier(resourceId));
                if (r == null)
                    throw new Exception($"Could not find service bus with resource id '{resourceId}'.");

                var topics = r.GetServiceBusTopics();
                if (!topics.Select(t => t.Data.Name).Contains(topicName, StringComparer.OrdinalIgnoreCase))
                    r.CreateTopic(topicName);

                topics = r.GetServiceBusTopics();

                _AvailableTopics.Clear();
                _AvailableTopics.AddRange(topics.Select(t => t.Data.Name));
            }
        }

        public static void CreateTopicSubscriptionIfNotExists(this ArmClient _ManagerClient, string topicName, string resourceId, string subscriptionName)
        {
            _ManagerClient.CreateTopicIfNotExists(topicName, resourceId);

            List<string> subscriptions = GetTopicSubscriptions(topicName);
            if (subscriptions.Contains(subscriptionName, StringComparer.OrdinalIgnoreCase))
                return;

            lock (_SubscriptionCreateLock)
            {
                if (subscriptions.Contains(subscriptionName, StringComparer.OrdinalIgnoreCase))
                    return;

                ServiceBusNamespaceResource r = _ManagerClient.GetServiceBusNamespaceResource(new ResourceIdentifier(resourceId));
                if (r == null)
                    throw new Exception($"Could not find service bus with resource id '{resourceId}'.");

                var topics = r.GetServiceBusTopics();
                var foundTopic = topics.FirstOrDefault(t => t.Data.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase));
                if (foundTopic == null)
                    throw new Exception($"Could not find service bus topic '{topicName}' within resource '{resourceId}'.");

                var foundSubscriptions = foundTopic.GetServiceBusSubscriptions();
                if (!foundSubscriptions.Select(s => s.Data.Name).Contains(subscriptionName, StringComparer.OrdinalIgnoreCase))
                    foundTopic.CreateTopicSubscription(subscriptionName);

                foundSubscriptions = foundTopic.GetServiceBusSubscriptions();
                var newList = foundSubscriptions.Select(s => s.Data.Name).ToList();
                _AvailableSubscriptionsPerTopic.AddOrUpdate(topicName, (t) => newList, (t, l) => newList);
            }
        }

        private static List<string> GetTopicSubscriptions(string topicName)
        {
            List<string> subscriptions = new List<string>();
            if (!_AvailableSubscriptionsPerTopic.TryAdd(topicName, subscriptions))
                return _AvailableSubscriptionsPerTopic[topicName];

            return subscriptions;
        }

        public static void CreateTopic(this ServiceBusNamespaceResource resource, string topicName)
        {
            var topics = resource.GetServiceBusTopics();
            topics.CreateOrUpdate(WaitUntil.Completed, topicName, new ServiceBusTopicData
            {
                EnableBatchedOperations = true,
                MaxMessageSizeInKilobytes = 1024,
                MaxSizeInMegabytes = 1024,
            });
        }

        public static void CreateTopicSubscription(this ServiceBusTopicResource resource, string subscriptionName)
        {
            var subscriptions = resource.GetServiceBusSubscriptions();
            subscriptions.CreateOrUpdate(WaitUntil.Completed, subscriptionName, new ServiceBusSubscriptionData
            {
                // TODO: What are the default values here?
            });
        }

        public static string GetSubjectName<T>()
            => $"{GetTopicName<T>()}/messages/{typeof(T).FullName}";

        public static string GetTopicName<T>()
            => GetTopic<T>().QueueName;

        public static QueueNameAttribute GetTopic<T>()
            => GetTopic(typeof(T));

        public static QueueNameAttribute GetTopic(Type type)
            => type.GetCustomAttribute<QueueNameAttribute>()
                    ?? throw new Exception($"QueueNameAttribute is required on the type '{type.Name}'.");

        public static ServiceBusSubscriptionAttribute GetSubscription<T>()
            => GetSubscription(typeof(T));

        public static ServiceBusSubscriptionAttribute GetSubscription(Type type)
            => type.GetCustomAttribute<ServiceBusSubscriptionAttribute>()
                    ?? throw new Exception($"ServiceBusSubscriptionAttribute is required on the type '{type.Name}'.");

        private const string MESSAGE_TYPE_KEY = "MessageType";
        public static string GetMessageType(this ServiceBusReceivedMessage message)
        {
            if (!message.ApplicationProperties.ContainsKey(MESSAGE_TYPE_KEY))
                return string.Empty;

            return message.ApplicationProperties[MESSAGE_TYPE_KEY]?.ToString() ?? string.Empty;
        }

        public static void SetMessageType(this ServiceBusMessage message, string messageType)
        {
            message.ApplicationProperties.Add(MESSAGE_TYPE_KEY, messageType);
        }
    }
}
