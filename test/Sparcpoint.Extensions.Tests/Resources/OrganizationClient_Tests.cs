using Sparcpoint.Extensions.Resources;
using Sparcpoint.Extensions.Resources.InMemory;

namespace Sparcpoint.Extensions.Tests.Resources
{
    public class OrganizationClient_Tests
    {
        public OrganizationClient_Tests()
        {
            Factory = new SparcpointClientFactory(new InMemoryResourceStore(new InMemoryResourceCollection()));
            Client = Factory.Create("ACCOUNT_01");
            Subscription = Client.CreateNewSubscriptionAsync(new SubscriptionData { DisplayName = "SUB 1" }).Result;
            Organization = Subscription.CreateNewOrganizationAsync(new OrganizationData { DisplayName = "ORG 1" }).Result;
        }

        public SparcpointClientFactory Factory { get; }
        public ISparcpointClient Client { get; }
        public ISubscriptionClient Subscription { get; }
        public IOrganizationClient Organization { get; }
    }
}
