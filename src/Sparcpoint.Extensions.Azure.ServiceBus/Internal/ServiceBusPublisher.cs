using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Sparcpoint.Queues;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Azure.ServiceBus.Internal
{
    internal class ServiceBusPublisher<T> : IPublisher<T>
        where T : class, new()
    {
        private readonly IOptions<ServiceBusOptions> _Options;

        private string _SubjectName = null;
        private string _TopicName = null;
        private ServiceBusClient _ServiceBusClient = null;

        public ServiceBusPublisher(IOptions<ServiceBusOptions> options)
        {
            _Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task PublishAsync(T message, CancellationToken cancellationToken = default)
        {
            string topicName = GetTopicName();
            ServiceBusClient client = GetClient();

            var queueMessage = new ServiceBusMessageDto<T>
            {
                MessageType = typeof(T).FullName,
                EnqueueTime = DateTimeOffset.UtcNow,
                Message = message
            };
            var json = JsonSerializer.Serialize(queueMessage);
            var messageId = Guid.NewGuid().ToString();

            await using var sender = client.CreateSender(topicName);
            await sender.SendMessageAsync(new ServiceBusMessage
            {
                ContentType = "application/json",
                Body = BinaryData.FromString(json),
                MessageId = messageId,
                Subject = GetSubjectName(),
            });
        }

        private string GetSubjectName()
            => _SubjectName ?? (_SubjectName = ServiceBusHelpers.GetSubjectName<T>());

        private string GetTopicName()
            => _TopicName ?? (_TopicName = ServiceBusHelpers.GetTopicName<T>());

        private ServiceBusClient GetClient()
            => _ServiceBusClient ?? (_ServiceBusClient = new ServiceBusClient(_Options.Value?.ConnectionString));
    }
}
