using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sparcpoint.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Azure.ServiceBus.Internal
{
    internal class ServiceBusMessageProcessor<T> : IHostedService
    {
        private readonly IOptions<_ServiceBusProcessorOptions> _Options;
        private readonly IEnumerable<ISubscriber<T>> _Subscribers;

        private string _TopicName = null;
        private string _SubjectName = null;
        private ServiceBusClient _ServiceBusClient = null;
        private ServiceBusProcessor _ServiceBusProcessor = null;

        public ServiceBusMessageProcessor(IOptions<_ServiceBusProcessorOptions> options, IEnumerable<ISubscriber<T>> subscribers)
        {
            _Options = options ?? throw new ArgumentNullException(nameof(options));
            _Subscribers = subscribers ?? throw new ArgumentNullException(nameof(subscribers));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_Subscribers?.Count() == 0)
                return;

            if (_ServiceBusProcessor != null)
                throw new Exception("Processor already started.");

            var topicName = GetTopicName();
            var client = GetClient();
            _ServiceBusProcessor = client.CreateProcessor(topicName, _Options.Value.SubscriptionName, new ServiceBusProcessorOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });

            _ServiceBusProcessor.ProcessMessageAsync += _ServiceBusProcessor_ProcessMessageAsync;
            _ServiceBusProcessor.ProcessErrorAsync += _ServiceBusProcessor_ProcessErrorAsync;

            await _ServiceBusProcessor.StartProcessingAsync(cancellationToken).ConfigureAwait(false);
        }

        private Task _ServiceBusProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            // TODO: How do we want to handle errors for this processor?
            return Task.CompletedTask;
        }

        private async Task _ServiceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var subjectName = GetSubjectName();
            if (!arg.Message.Subject.Equals(subjectName) || !arg.Message.ContentType.Equals("application/json"))
            {
                await arg.DeferMessageAsync(arg.Message).ConfigureAwait(false);
                return;
            }

            string json = arg.Message.Body.ToString();
            ServiceBusMessageDto<T> sbMessage = JsonSerializer.Deserialize<ServiceBusMessageDto<T>>(json);

            List<Task> subscribers = new List<Task>();
            foreach (var subscriber in _Subscribers)
            {
                // Create a new message for each subscriber so a subscriber cannot affect other subscriber data
                var queuedMessage = new QueueMessage<T>
                {
                    EnqueueTime = sbMessage.EnqueueTime,
                    Message = sbMessage.Message,
                    QueueName = GetTopicName(),
                };

                queuedMessage.Properties.Add("Namespace", arg.FullyQualifiedNamespace);
                queuedMessage.Properties.Add("Subject", arg.Message.Subject);
                queuedMessage.Properties.Add("Identifier", arg.Identifier);
                queuedMessage.Properties.Add("ContentType", arg.Message.ContentType);

                subscribers.Add(ReceiveMessageAsync(subscriber, queuedMessage, arg.CancellationToken));
            }

            await Task.WhenAll(subscribers).ConfigureAwait(false);
            await arg.CompleteMessageAsync(arg.Message, arg.CancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_ServiceBusProcessor == null)
                return;

            await _ServiceBusProcessor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);
            _ServiceBusProcessor = null;
        }

        private async Task ReceiveMessageAsync(ISubscriber<T> subscriber, QueueMessage<T> message, CancellationToken cancellationToken)
        {
            try
            {
                await subscriber.OnMessageReceivedAsync(message, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // TODO: What do we do when a subscriber throws up?
            }
        }

        private string GetSubjectName()
            => _SubjectName ?? (_SubjectName = ServiceBusHelpers.GetSubjectName<T>());

        private string GetTopicName()
            => _TopicName ?? (_TopicName = ServiceBusHelpers.GetTopicName<T>());

        private ServiceBusClient GetClient()
            => _ServiceBusClient ?? (_ServiceBusClient = new ServiceBusClient(_Options.Value?.ConnectionString));
    }
}
