using Sparcpoint.Extensions.Resources;
using Sparcpoint.Extensions.Resources.InMemory;

namespace Sparcpoint.Extensions.Tests.Resources
{
    public class SparcpointClient_Tests
    {
        public SparcpointClient_Tests()
        {
            Factory = new SparcpointClientFactory(new InMemoryResourceStore(new InMemoryResourceCollection()));
        }

        public SparcpointClientFactory Factory { get; }

        [Fact]
        public void Can_create_client()
        {
            var client = Factory.Create("ACCOUNT_01");
            Assert.NotNull(client);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Given_bad_account_id_throws(string? accountId)
        {
            Assert.ThrowsAny<ArgumentException>(() => Factory.Create(accountId!));
        }

        [Fact]
        public async Task Can_create_new_subscription_and_get()
        {
            var subscription = await CreateSubscription("ACCOUNT_02", "SUB 2");
            Assert.NotNull(subscription);

            var value = await subscription.GetAsync();
            Assert.NotNull(value);
            Assert.Equal("SUB 2", value.DisplayName);
            Assert.Equal("ACCOUNT_02", value.OwnerAccountId);
        }

        [Fact]
        public async Task Given_two_accounts_cannot_see_other_subscriptions()
        {
            await CreateSubscription("ACCOUNT_01", "SUB 1");
            await CreateSubscription("ACCOUNT_02", "SUB 2");

            var client = Factory.Create("ACCOUNT_01");
            var allSubs = await client.GetSubscriptionsAsync().ToArrayAsync();
            Assert.Single(allSubs);
            var sub_get = await allSubs[0].GetAsync();
            Assert.NotNull(sub_get);
            Assert.Equal("ACCOUNT_01", sub_get.OwnerAccountId);
            Assert.Equal("SUB 1", sub_get.DisplayName);

            client = Factory.Create("ACCOUNT_02");
            allSubs = await client.GetSubscriptionsAsync().ToArrayAsync();
            Assert.Single(allSubs);
            sub_get = await allSubs[0].GetAsync();
            Assert.NotNull(sub_get);
            Assert.Equal("ACCOUNT_02", sub_get.OwnerAccountId);
            Assert.Equal("SUB 2", sub_get.DisplayName);
        }

        [Fact]
        public async Task Given_multiple_subs_all_returned()
        {
            await CreateSubscription("ACCOUNT_01", "SUB 1");
            await CreateSubscription("ACCOUNT_01", "SUB 2");
            await CreateSubscription("ACCOUNT_01", "SUB 3");

            var client = Factory.Create("ACCOUNT_01");
            var allSubs = await client.GetSubscriptionsAsync().ToArrayAsync();

            Assert.Equal(3, allSubs.Length);
            await AssertDisplayName(allSubs[0], "SUB 1");
            await AssertDisplayName(allSubs[1], "SUB 2");
            await AssertDisplayName(allSubs[2], "SUB 3");
        }

        [Fact]
        public async Task Given_duplicate_subscription_display_names_and_same_account_throws()
        {
            await CreateSubscription("ACCOUNT_01", "SUB 1");
            await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await CreateSubscription("ACCOUNT_01", "SUB 1"));
        }

        [Fact]
        public async Task Given_duplicate_subscription_display_names_and_different_accounts_does_not_throw()
        {
            await CreateSubscription("ACCOUNT_01", "SUB 1");
            await CreateSubscription("ACCOUNT_02", "SUB 1");
        }

        private async Task<ISubscriptionClient> CreateSubscription(string accountId, string displayName)
        {
            var client = Factory.Create(accountId);
            return await client.CreateNewSubscriptionAsync(new SubscriptionData { DisplayName = displayName, OwnerAccountId = "XXXXX" });
        }

        private async Task AssertDisplayName(ISubscriptionClient client, string displayName)
        {
            var sub = await client.GetAsync();
            Assert.NotNull(sub);
            Assert.Equal(displayName, sub.DisplayName);
        }
    }
}
