using Sparcpoint.Extensions.Permissions;
using Sparcpoint.Extensions.Resources;
using Sparcpoint.Extensions.Resources.InMemory;

namespace Sparcpoint.Extensions.Tests.Resources
{
    public class SubscriptionClient_Tests
    {
        public SubscriptionClient_Tests()
        {
            Factory = new SparcpointClientFactory(new InMemoryResourceStore(new InMemoryResourceCollection()));
            Client = Factory.Create("ACCOUNT_01");
            Subscription = Client.CreateNewSubscriptionAsync(new SubscriptionData { DisplayName = "SUB 1" }).Result;
        }

        public SparcpointClientFactory Factory { get; }
        public ISparcpointClient Client { get; }
        public ISubscriptionClient Subscription { get; }

        [Fact]
        public async Task Can_delete_confirmed()
        {
            await Subscription.DeleteAsync();

            var allSubs = await Client.GetSubscriptionsAsync().ToArrayAsync();
            Assert.Empty(allSubs);
        }

        [Fact]
        public async Task Can_get_permissions()
        {
            var permissions = await Subscription.GetPermissionsAsync();
            Assert.NotNull(permissions);
            Assert.Equal(4, permissions.Count);

            AssertIsAllowed(permissions, "ACCOUNT_01", CommonPermissions.CanReadData);
            AssertIsAllowed(permissions, "ACCOUNT_01", CommonPermissions.CanReadPermissions);
            AssertIsAllowed(permissions, "ACCOUNT_01", CommonPermissions.CanWriteData);
            AssertIsAllowed(permissions, "ACCOUNT_01", CommonPermissions.CanWritePermissions);
        }

        [Fact]
        public async Task Can_set_permissions()
        {
            var permissions = new ResourcePermissions
            {
                new ResourcePermissionEntry { AccountId = "ACCOUNT_02", Permission = new PermissionEntry("ToLive", PermissionValue.Allow) }
            };
            await Subscription.SetPermissionsAsync(permissions);

            var actualPermissions = await Subscription.GetPermissionsAsync();
            Assert.NotNull(actualPermissions);
            Assert.Single(actualPermissions);
            AssertIsAllowed(actualPermissions, "ACCOUNT_02", "ToLive");
        }

        [Fact]
        public async Task Can_get_child_resource()
        {
            var org = await CreateOrganization("ORG 1");
            Assert.NotNull(org);

            var child = Subscription.GetChildClient<OrganizationData>(org.ResourceId - Subscription.ResourceId);
            Assert.NotNull(child);
            var foundOrg = await child.GetAsync();
            Assert.NotNull(foundOrg);
            Assert.Equal("ORG 1", foundOrg.DisplayName);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(10)]
        public async Task Can_get_multiple_children_resources(int maxDepth)
        {
            await CreateOrganization("ORG 1");
            await CreateOrganization("ORG 2");
            await CreateOrganization("ORG 3");

            var found = await Subscription
                .GetChildClientsAsync<OrganizationData>(maxDepth)
                .Select(async (c) => await c.GetAsync())
                .Where(c => c != null)
                .ToArrayAsync();

            Assert.NotNull(found);
            Assert.Equal(3, found.Length);
            Assert.Contains(found, (c) => c!.DisplayName == "ORG 1");
            Assert.Contains(found, (c) => c!.DisplayName == "ORG 2");
            Assert.Contains(found, (c) => c!.DisplayName == "ORG 3");
        }

        [Fact]
        public async Task Can_create_new_organization_and_get()
        {
            var org = await CreateOrganization("ORG 1");
            Assert.NotNull(org);

            var value = await org.GetAsync();
            Assert.NotNull(value);
            Assert.Equal("ORG 1", value.DisplayName);
        }

        [Fact]
        public async Task Duplicate_organization_display_name_in_same_subscription_throws()
        {
            await CreateOrganization("ORG 1");
            await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await CreateOrganization("ORG 1"));
        }

        [Fact]
        public async Task Duplicate_organization_display_name_in_different_subscriptions_does_not_throw()
        {
            await CreateOrganization("ORG 1");

            var sub2 = await Client.CreateNewSubscriptionAsync(new SubscriptionData { DisplayName = "SUB 2" });
            await CreateOrganization("ORG 1", sub2);
        }

        [Fact]
        public async Task Given_multiple_organizations_all_return()
        {
            await CreateOrganization("ORG 1");
            await CreateOrganization("ORG 2");
            await CreateOrganization("ORG 3");

            var orgs = await Subscription.GetOrganizationsAsync().Select(async o => await o.GetAsync()).ToArrayAsync();
            Assert.NotNull(orgs);
            Assert.NotEmpty(orgs);
            Assert.Contains(orgs, o => o!.DisplayName == "ORG 1");
            Assert.Contains(orgs, o => o!.DisplayName == "ORG 2");
            Assert.Contains(orgs, o => o!.DisplayName == "ORG 3");
        }

        private async Task<IOrganizationClient> CreateOrganization(string displayName, ISubscriptionClient? sub = null)
        {
            return await (sub ?? Subscription).CreateNewOrganizationAsync(new OrganizationData { DisplayName = displayName });
        }

        private void AssertIsAllowed(ResourcePermissions permissions, string accountId, string key)
            => Assert.Contains(permissions, (p) => p.AccountId == accountId && p.Permission.Key == key && (p.Permission.IsAllowed ?? false));
    }
}
