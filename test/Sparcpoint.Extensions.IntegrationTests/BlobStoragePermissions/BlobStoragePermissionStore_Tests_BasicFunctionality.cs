using Sparcpoint.Extensions.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.IntegrationTests.BlobStoragePermissions;

public class BlobStoragePermissionStore_Tests_BasicFunctionality : IClassFixture<BlobStorageFixture>
{
    private readonly BlobStorageFixture _Fixture;

    public BlobStoragePermissionStore_Tests_BasicFunctionality(BlobStorageFixture fixture)
    {
        _Fixture = fixture;
    }

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
}
