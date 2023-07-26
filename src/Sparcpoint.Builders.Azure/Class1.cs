using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.ServiceBus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sparcpoint.Builders.Azure
{
    internal delegate Task TaskDelegate<TResourceType>(TResourceType resource);
    internal delegate Task<TSubResourceType> SubResourceGetter<TParentResourceType, TSubResourceType>(TParentResourceType parentResource, string name);
    internal delegate Task<TSubResourceType> SubResourceCreator<TParentResourceType, TSubResourceType>(TParentResourceType parentResource, string name, object parameters);

    internal interface IBuilderWithTasks<TResourceType>
    {
        List<TaskDelegate<TResourceType>> Tasks { get; }
    }

    internal static class BuilderHelpers
    {
        public static void NamedResource<TParentResourceType, TSubResourceType, TSubBuilder, TImplSubBuilder>(
            SubResourceGetter<TParentResourceType, TSubResourceType> subResourceGetter, 
            SubResourceCreator<TParentResourceType, TSubResourceType> subResourceCreator,
            string name, Func<TSubBuilder, TSubBuilder> b, List<TaskDelegate<TParentResourceType>> tasks)
            where TImplSubBuilder : TSubBuilder, IBuilderWithTasks<TSubResourceType>, new()
        {
            var builder = new TImplSubBuilder();
            b(builder);

            tasks.Add(async (resource) =>
            {
                TSubResourceType subResource = await subResourceGetter(resource, name);
                if (subResource == null)
                {
                    subResource = await subResourceCreator(resource, name, null);
                }

                foreach(var t in builder.Tasks)
                {
                    await t(subResource);
                }
            });
        }
    }

    internal class DefaultResourceGroupBuilder : IResourceGroupBuilder
    {
        public delegate Task ResourceGroupTask(ResourceGroupResource resource);
        public List<TaskDelegate<ResourceGroupResource>> Tasks { get; } = new List<TaskDelegate<ResourceGroupResource>>();

        public IResourceGroupBuilder ServiceBus(string namespaceName, Func<IServiceBusBuilder, IServiceBusBuilder> sb)
        {
            BuilderHelpers.NamedResource<ResourceGroupResource, ServiceBusNamespaceResource, IServiceBusBuilder, DefaultServiceBusBuilder>
                (GetServiceBusNamespace, CreateServiceBusNamespace, namespaceName, sb, Tasks);

            return this;
        }

        private async Task<ServiceBusNamespaceResource> GetServiceBusNamespace(ResourceGroupResource resource, string serviceBusName)
        {
            var list = resource.GetServiceBusNamespaces();
            if (await list.ExistsAsync(serviceBusName))
                return await list.GetAsync(serviceBusName);

            return null;
        }

        private async Task<ServiceBusNamespaceResource> CreateServiceBusNamespace(ResourceGroupResource resource, string serviceBusNamespace, object parameters)
        {
            var list = resource.GetServiceBusNamespaces();
            var op = await list.CreateOrUpdateAsync(global::Azure.WaitUntil.Completed, serviceBusNamespace, new ServiceBusNamespaceData(global::Azure.Core.AzureLocation.EastUS)
            {

            });
            var response = await op.WaitForCompletionAsync();
            return response.Value;
        }

    }

    internal class DefaultServiceBusBuilder : IServiceBusBuilder, IBuilderWithTasks<ServiceBusNamespaceResource>
    {
        public List<TaskDelegate<ServiceBusNamespaceResource>> Tasks { get; }

        public IServiceBusBuilder Queue(string queueName)
        {
            Tasks.Add(async (resource) =>
            {
                var queues = resource.GetServiceBusQueues();
                if (!await queues.ExistsAsync(queueName))
                {
                    await queues.CreateOrUpdateAsync(global::Azure.WaitUntil.Completed, queueName, new ServiceBusQueueData
                    {
                        // TODO: Parameters
                    });
                }
            });

            return this;
        }

        public IServiceBusBuilder Topic(string topicName, Func<IServiceBusTopicSubscriptionBuilder, IServiceBusTopicSubscriptionBuilder> topic)
        {
            var builder = new DefaultServiceBusTopicSubscriptionBuilder();
            topic(builder);

            Tasks.Add(async (resource) =>
            {
                var topics = resource.GetServiceBusTopics();
                ServiceBusTopicResource topicResource;
                if (!await topics.ExistsAsync(topicName))
                {
                    var op = await topics.CreateOrUpdateAsync(global::Azure.WaitUntil.Completed, topicName, new ServiceBusTopicData
                    {
                        // TODO: Parameters
                    });
                    var response = await op.WaitForCompletionAsync();
                    topicResource = response.Value;
                } else
                {
                    topicResource = await topics.GetAsync(topicName);
                }

                foreach (var task in builder.Tasks)
                {
                    await task(topicResource);
                }
            });

            return this;
        }
    }

    internal class DefaultServiceBusTopicSubscriptionBuilder : IServiceBusTopicSubscriptionBuilder
    {
        public delegate Task ServiceBusTopicTask(ServiceBusTopicResource resource);
        public List<ServiceBusTopicTask> Tasks { get; } = new List<ServiceBusTopicTask>();

        public IServiceBusTopicSubscriptionBuilder Subscription(string subscriptionName)
        {
            Tasks.Add(async (resource) =>
            {
                var subs = resource.GetServiceBusSubscriptions();
                if (!await subs.ExistsAsync(subscriptionName))
                {
                    await subs.CreateOrUpdateAsync(global::Azure.WaitUntil.Completed, subscriptionName, new ServiceBusSubscriptionData
                    {
                        // TODO: Parameters
                    });
                }
            });

            return this;
        }
    }

    public interface IResourceGroupBuilder
    {
        IResourceGroupBuilder ServiceBus(string namespaceName, Func<IServiceBusBuilder, IServiceBusBuilder> sb);
    }

    public interface IServiceBusBuilder
    {
        IServiceBusBuilder Queue(string queueName);
        IServiceBusBuilder Topic(string topicName, Func<IServiceBusTopicSubscriptionBuilder, IServiceBusTopicSubscriptionBuilder> topic);
    }

    public interface IServiceBusTopicSubscriptionBuilder
    {
        IServiceBusTopicSubscriptionBuilder Subscription(string subscriptionName);
    }
}
