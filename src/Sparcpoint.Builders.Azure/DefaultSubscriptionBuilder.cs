using Azure.ResourceManager.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sparcpoint.Builders.Azure
{
    internal class DefaultSubscriptionBuilder : ISubscriptionBuilder
    {
        public delegate Task SubscriptionTask(SubscriptionResource subscription);
        public List<SubscriptionTask> Tasks { get; } = new List<SubscriptionTask>();

        public ISubscriptionBuilder ResourceGroup(string resourceGroupName, Func<IResourceGroupBuilder, IResourceGroupBuilder> rg)
        {
            var builder = new DefaultResourceGroupBuilder();
            rg(builder);
            Tasks.Add(async (sub) =>
            {
                var resourceGroups = sub.GetResourceGroups();
                ResourceGroupResource group;
                if (!await resourceGroups.ExistsAsync(resourceGroupName))
                {
                    var op = await resourceGroups.CreateOrUpdateAsync(global::Azure.WaitUntil.Completed, resourceGroupName, new ResourceGroupData(global::Azure.Core.AzureLocation.EastUS));
                    var response = await op.WaitForCompletionAsync();
                    group = response.Value;
                } else
                {
                    group = await resourceGroups.GetAsync(resourceGroupName);
                }

                foreach(var task in builder.Tasks)
                {
                    await task(group);
                }
            });

            return this;
        }
    }
}
