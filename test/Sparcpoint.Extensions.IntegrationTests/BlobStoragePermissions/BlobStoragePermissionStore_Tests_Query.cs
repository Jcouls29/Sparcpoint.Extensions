using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.IntegrationTests.BlobStoragePermissions;

[Collection("Blob Storage")]
public class BlobStoragePermissionStore_Tests_Query
{
    private readonly BlobStorageFixture _Fixture;

    public BlobStoragePermissionStore_Tests_Query(BlobStorageFixture fixture)
    {
        _Fixture = fixture;
    }

    public IPermissionStore Store => _Fixture.PermissionStore;
    public IAccountPermissionQuery Query => _Fixture.PermissionQuery;

    [Fact]
    public async Task KeyExact_Only()
    {
        var parameters = new PermissionQueryParameters { AccountId = "acct_001", KeyExact = BlobStorageFixture.P_READ };

        var entries = await Query.RunAsync(parameters).ToListAsync();

        Assert.Equal(3, entries.Count);
        Allowed(entries, "acct_001", "/", BlobStorageFixture.P_READ);
        Allowed(entries, "acct_001", "/organizations", BlobStorageFixture.P_READ);
        Allowed(entries, "acct_001", "/organizations/projects/blues-brothers", BlobStorageFixture.P_READ);
    }

    [Fact]
    public async Task KeyStartsWith_Only()
    {
        var parameters = new PermissionQueryParameters { AccountId = "acct_001", KeyStartsWith = "Can" };

        var entries = await Query.RunAsync(parameters).ToListAsync();

        Assert.Equal(6, entries.Count);
        Allowed(entries, "acct_001", "/", BlobStorageFixture.P_READ);
        Allowed(entries, "acct_001", "/organizations", BlobStorageFixture.P_READ);
        Allowed(entries, "acct_001", "/organizations/projects/blues-brothers", BlobStorageFixture.P_READ);
        Denied(entries, "acct_001", "/", BlobStorageFixture.P_WRITE);
        Denied(entries, "acct_001", "/organizations", BlobStorageFixture.P_WRITE);
        Denied(entries, "acct_001", "/organizations/projects/blues-brothers", BlobStorageFixture.P_WRITE);
    }

    [Fact]
    public async Task KeyEndsWith_Only()
    {
        var parameters = new PermissionQueryParameters { AccountId = "acct_001", KeyEndsWith = "Write" };

        var entries = await Query.RunAsync(parameters).ToListAsync();

        Assert.Equal(3, entries.Count);
        Denied(entries, "acct_001", "/", BlobStorageFixture.P_WRITE);
        Denied(entries, "acct_001", "/organizations", BlobStorageFixture.P_WRITE);
        Denied(entries, "acct_001", "/organizations/projects/blues-brothers", BlobStorageFixture.P_WRITE);
    }

    [Fact]
    public async Task ScopeExact_Only()
    {
        var parameters = new PermissionQueryParameters { AccountId = "acct_001", ScopeExact = "/" };

        var entries = await Query.RunAsync(parameters).ToListAsync();

        Assert.Equal(2, entries.Count);
        Allowed(entries, "acct_001", "/", BlobStorageFixture.P_READ);
        Denied(entries, "acct_001", "/", BlobStorageFixture.P_WRITE);
    }

    [Fact]
    public async Task ScopeStartsWith_Only()
    {
        var parameters = new PermissionQueryParameters { AccountId = "acct_001", ScopeStartsWith = "/organizations" };

        var entries = await Query.RunAsync(parameters).ToListAsync();

        Assert.Equal(4, entries.Count);
        Allowed(entries, "acct_001", "/organizations", BlobStorageFixture.P_READ);
        Allowed(entries, "acct_001", "/organizations/projects/blues-brothers", BlobStorageFixture.P_READ);
        Denied(entries, "acct_001", "/organizations", BlobStorageFixture.P_WRITE);
        Denied(entries, "acct_001", "/organizations/projects/blues-brothers", BlobStorageFixture.P_WRITE);
    }

    [Fact]
    public async Task ScopeEndsWith_Only()
    {
        var parameters = new PermissionQueryParameters { AccountId = "acct_001", ScopeEndsWith = "organizations" };

        var entries = await Query.RunAsync(parameters).ToListAsync();

        Assert.Equal(2, entries.Count);
        Allowed(entries, "acct_001", "/organizations", BlobStorageFixture.P_READ);
        Denied(entries, "acct_001", "/organizations", BlobStorageFixture.P_WRITE);
    }

    [Fact]
    public async Task ValueExact_Only()
    {
        var parameters = new PermissionQueryParameters { AccountId = "acct_001", ValueExact = PermissionValue.Allow };

        var entries = await Query.RunAsync(parameters).ToListAsync();

        Assert.Equal(3, entries.Count);
        Allowed(entries, "acct_001", "/", BlobStorageFixture.P_READ);
        Allowed(entries, "acct_001", "/organizations", BlobStorageFixture.P_READ);
        Allowed(entries, "acct_001", "/organizations/projects/blues-brothers", BlobStorageFixture.P_READ);
    }

    [Fact]
    public async Task WithMetadata_Only()
    {
        var parameters = new PermissionQueryParameters { AccountId = "acct_001", WithMetadata = new Dictionary<string, string> { ["Color"] = "Blue" } };
        var entries = await Query.RunAsync(parameters).ToListAsync();

        Assert.Single(entries);
        Allowed(entries, "acct_001", "/", BlobStorageFixture.P_READ);
    }

    [Fact]
    public async Task NoAccount_KeyExact_ScopeExact()
    {
        var parameters = new PermissionQueryParameters { KeyExact = BlobStorageFixture.P_READ, ScopeExact = "/" };
        var entries = await Query.RunAsync(parameters).ToListAsync();

        Assert.Equal(4, entries.Count);

        Allowed(entries, "acct_001", "/", BlobStorageFixture.P_READ);
        Allowed(entries, "acct_002", "/", BlobStorageFixture.P_READ);
        Denied(entries, "acct_003", "/", BlobStorageFixture.P_READ);
        Allowed(entries, "acct_005", "/", BlobStorageFixture.P_READ);
    }

    [Fact]
    public async Task ImmediateChildrenCheck()
    {
        var parameters = new PermissionQueryParameters { ScopeStartsWith = "/orgs/ABC/projects", ImmediateChildrenOnly = true, KeyExact = BlobStorageFixture.P_READ, ValueExact = PermissionValue.Allow };
        var entries = await Query.RunAsync(parameters).ToListAsync();

        Assert.Equal(2, entries.Count);

        Allowed(entries, "acct_010", "/orgs/ABC/projects/project01", BlobStorageFixture.P_READ);
        Allowed(entries, "acct_010", "/orgs/ABC/projects/project03", BlobStorageFixture.P_READ);
    }

    private void AssertEntries(IEnumerable<AccountPermissionEntry> entries, string accountId, ScopePath scope, string key, PermissionValue value)
    {
        Assert.Contains(entries, p => p.AccountId == accountId && p.Scope == scope && p.Entry.Key == key && p.Entry.Value == value);
    }

    private void Allowed(IEnumerable<AccountPermissionEntry> entries, string accountId, ScopePath scope, string key)
    {
        AssertEntries(entries, accountId, scope, key, PermissionValue.Allow);
    }

    private void Denied(IEnumerable<AccountPermissionEntry> entries, string accountId, ScopePath scope, string key)
    {
        AssertEntries(entries, accountId, scope, key, PermissionValue.Deny);
    }
}
