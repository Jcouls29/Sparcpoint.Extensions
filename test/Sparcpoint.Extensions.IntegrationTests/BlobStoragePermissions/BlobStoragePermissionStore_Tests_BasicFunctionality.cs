using Sparcpoint.Extensions.Permissions;
using Sparcpoint.Extensions.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.IntegrationTests.BlobStoragePermissions;

[Collection("Blob Storage")]
public class BlobStoragePermissionStore_Tests_BasicFunctionality
{
    private readonly BlobStorageFixture _Fixture;

    public BlobStoragePermissionStore_Tests_BasicFunctionality(BlobStorageFixture fixture)
    {
        _Fixture = fixture;
    }

    public IPermissionStore Store => _Fixture.PermissionStore;
    public IScopePermissionCollection Collection => _Fixture.PermissionStore.Permissions;

    [Theory]
    [InlineData("/")]
    [InlineData("/organizations")]
    [InlineData("/organizations/projects/blues-brothers")]
    public async Task Can_add_and_retrieve_permissions(string scope)
    {
        var collGetter = _Fixture.PermissionStore.Get("acct_001", scope: scope);
        var canRead = await collGetter.FindAsync(BlobStorageFixture.P_READ);
        Assert.NotNull(canRead);
        Assert.Equal(Permissions.PermissionValue.Allow, canRead.Value);

        var canWrite = await collGetter.FindAsync(BlobStorageFixture.P_WRITE);
        Assert.NotNull(canWrite);
        Assert.Equal(Permissions.PermissionValue.Deny, canWrite.Value);
    }

    [Theory()]
    [InlineData("/")]
    [InlineData("/organizations")]
    [InlineData("/organizations/projects/blues-brothers")]
    public async Task Clear_account_does_not_affect_other_account(string scope)
    {
        // Setup
        var acct01 = _Fixture.PermissionStore.Get("acct_clear", scope: scope);
        await acct01.SetAsync(BlobStorageFixture.P_READ, Permissions.PermissionValue.Allow);
        await acct01.SetAsync(BlobStorageFixture.P_WRITE, Permissions.PermissionValue.Deny);
        // Verify Setup
        Assert.True(await acct01.IsAllowedDirect(BlobStorageFixture.P_READ));
        Assert.True(await acct01.IsDeniedDirect(BlobStorageFixture.P_WRITE));

        // Verify Test Account
        var acct02 = _Fixture.PermissionStore.Get("acct_002", scope: scope);
        Assert.True(await acct02.IsAllowedDirect(BlobStorageFixture.P_READ));
        Assert.True(await acct02.IsAllowedDirect(BlobStorageFixture.P_WRITE));

        // Perform
        await acct01.ClearAsync();
        // Verify Perform
        Assert.False(await acct01.ContainsAsync(BlobStorageFixture.P_READ));
        Assert.False(await acct01.ContainsAsync(BlobStorageFixture.P_WRITE));

        // Asert
        Assert.True(await acct02.IsAllowedDirect(BlobStorageFixture.P_READ));
        Assert.True(await acct02.IsAllowedDirect(BlobStorageFixture.P_WRITE));
    }

    [Theory()]
    [InlineData("/")]
    [InlineData("/organizations")]
    [InlineData("/organizations/projects/blues-brothers")]
    public async Task Remove_account_key_does_not_affect_other_account(string scope)
    {
        // Setup
        var acct01 = _Fixture.PermissionStore.Get("acct_clear", scope: scope);
        await acct01.SetAsync(BlobStorageFixture.P_READ, Permissions.PermissionValue.Allow);
        // Verify Setup
        Assert.True(await acct01.IsAllowedDirect(BlobStorageFixture.P_READ));

        // Verify Test Account
        var acct02 = _Fixture.PermissionStore.Get("acct_002", scope: scope);
        Assert.True(await acct02.IsAllowedDirect(BlobStorageFixture.P_READ));

        // Perform
        await acct01.RemoveAsync(BlobStorageFixture.P_READ);
        // Verify Perform
        Assert.False(await acct01.ContainsAsync(BlobStorageFixture.P_READ));

        // Asert
        Assert.True(await acct02.IsAllowedDirect(BlobStorageFixture.P_READ));
    }

    [Fact]
    public async Task Can_add_multiple_entries_to_different_scopes()
    {
        const string ACCOUNT_ID = "acct_mtest1";

        await Collection.SetRangeAsync("/organizations/org_01", b => b
            .Account(ACCOUNT_ID, e => e.Allow(BlobStorageFixture.P_READ).Deny(BlobStorageFixture.P_WRITE))
            .Scope("/rules/rule_01", s => s
                .Account(ACCOUNT_ID, e => e.Deny(BlobStorageFixture.P_READ).Allow(BlobStorageFixture.P_WRITE))
            )
        );

        Assert.True(await Collection.IsAllowed(ACCOUNT_ID, BlobStorageFixture.P_READ, "/organizations/org_01"));
        Assert.True(await Collection.IsDenied(ACCOUNT_ID, BlobStorageFixture.P_WRITE, "/organizations/org_01"));
        Assert.True(await Collection.IsDenied(ACCOUNT_ID, BlobStorageFixture.P_READ, "/rules/rule_01"));
        Assert.True(await Collection.IsAllowed(ACCOUNT_ID, BlobStorageFixture.P_WRITE, "/rules/rule_01"));
    }

    [Fact]
    public async Task Can_remove_multiple_entries_from_different_scopes()
    {
        const string ACCOUNT_ID = "acct_mtest2";

        var entries = await Collection.SetRangeAsync("/organizations/org_01", b => b
            .Account(ACCOUNT_ID, e => e.Allow(BlobStorageFixture.P_READ).Deny(BlobStorageFixture.P_WRITE))
            .Scope("/rules/rule_01", s => s
                .Account(ACCOUNT_ID, e => e.Deny(BlobStorageFixture.P_READ).Allow(BlobStorageFixture.P_WRITE))
            )
        );

        Assert.True(await Collection.IsAllowed(ACCOUNT_ID, BlobStorageFixture.P_READ, "/organizations/org_01"));
        Assert.True(await Collection.IsDenied(ACCOUNT_ID, BlobStorageFixture.P_WRITE, "/organizations/org_01"));
        Assert.True(await Collection.IsDenied(ACCOUNT_ID, BlobStorageFixture.P_READ, "/rules/rule_01"));
        Assert.True(await Collection.IsAllowed(ACCOUNT_ID, BlobStorageFixture.P_WRITE, "/rules/rule_01"));

        await Collection.RemoveAsync(entries);

        foreach(var e in entries)
        {
            Assert.False(await Collection.ContainsAsync(e));
        }
    }

    // TODO: View Tests
    [Theory]
    [InlineData("/organizations/projects/blues-brothers", "acct_001", true)]
    [InlineData("/organizations/projects/blues-brothers", "acct_002", true)]
    [InlineData("/organizations/projects/blues-brothers", "acct_003", true)]
    [InlineData("/organizations/projects/blues-brothers", "acct_004", true)]
    [InlineData("/organizations/projects/blues-brothers", "acct_005", true)]
    [InlineData("/organizations", "acct_001", true)]
    [InlineData("/organizations", "acct_002", true)]
    [InlineData("/organizations", "acct_003", false)]
    [InlineData("/organizations", "acct_004", null)]
    [InlineData("/organizations", "acct_005", true)]
    [InlineData("/", "acct_001", true)]
    [InlineData("/", "acct_002", true)]
    [InlineData("/", "acct_003", false)]
    [InlineData("/", "acct_004", null)]
    [InlineData("/", "acct_005", true)]
    [InlineData("/organizations/widgets", "acct_001", true)]
    [InlineData("/organizations/widgets", "acct_002", true)]
    [InlineData("/organizations/widgets", "acct_003", false)]
    [InlineData("/organizations/widgets", "acct_004", null)]
    [InlineData("/organizations/widgets", "acct_005", true)]
    public async Task All_accounts_retrieved_at_deep_scope(string scope, string acct, bool? isAllowed)
    {
        var entries = (await Store.GetView(ScopePath.Parse(scope), includeRootScope: true).ToListAsync());
        if (isAllowed == null)
            Assert.DoesNotContain(entries, p => p.AccountId == acct && p.Entry.Key == BlobStorageFixture.P_READ && (p.Entry.IsAllowed == isAllowed));
        else
            Assert.Contains(entries, p => p.AccountId == acct && p.Entry.Key == BlobStorageFixture.P_READ && (p.Entry.IsAllowed == isAllowed));
    }
}
