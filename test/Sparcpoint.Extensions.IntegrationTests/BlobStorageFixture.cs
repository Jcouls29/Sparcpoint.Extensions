using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sparcpoint.Extensions.Azure.Permissions;
using Sparcpoint.Extensions.Permissions;
using System.Reflection;

namespace Sparcpoint.Extensions.IntegrationTests;

public class BlobStorageFixture : IAsyncLifetime
{
    public const string P_READ = "CanRead";
    public const string P_WRITE = "CanWrite";

    public IObjectStore<BasicObject> ObjectStore { get; }
    public IObjectQuery<BasicObject> ObjectQuery { get; }

    public IPermissionStore PermissionStore { get; }
    public IAccountPermissionQuery PermissionQuery { get; }

    private string ConnectionString { get; }

    public BlobStorageFixture()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        ConnectionString = config["BlobStorage:ConnectionString"];

        var services = new ServiceCollection();
        services.AddBlobStorageObjects(new Azure.Objects.BlobStorage.BlobStorageObjectStoreOptions { ConnectionString = ConnectionString, ContainerName = "objects", Filename = ".object" });
        services.AddBlobStoragePermissions(new BlobStoragePermissionStoreOptions { ConnectionString = ConnectionString, ContainerName = "permissions", Filename = ".permissions" });
        var provider = services.BuildServiceProvider();

        ObjectStore = provider.GetRequiredService<IObjectStore<BasicObject>>();
        ObjectQuery = provider.GetRequiredService<IObjectQuery<BasicObject>>();

        PermissionStore = provider.GetRequiredService<IPermissionStore>();
        PermissionQuery = provider.GetRequiredService<IAccountPermissionQuery>();

        SeedObjects();
        _Permissions = SeedPermissions().ToList();
    }

    private readonly List<BasicObject> _Objects = new();
    private void SeedObjects()
    {
        CreateObject("/organizations/org_01", "Organization 01", "My Organization");
        CreateObject("/organizations/org_01/projects/prj_01", "Project 01", "My Project");
        CreateObject("/organizations/org_01/projects/prj_02", "Project 02", "My Project");
        CreateObject("/organizations/org_01/projects/prj_03", "Project 03", "My Project");

        CreateObject("/organizations/org_02", "Organization 02", "My Organization");
        CreateObject("/organizations/org_02/projects/prj_01", "Project 04", "My Project");

        CreateObject("/organizations/org_03", "Organization 03", "Other Organization");
        CreateObject("/organizations/org_03/projects/prj_01", "Project 01", "Other Project");

        CreateObject("/organizations/org_04", "Organization 04", "Other Organization");

        CreateObject("/rules/rule_01", "Rule 01", null);
    }

    private readonly List<AccountPermissionEntry> _Permissions;
    private IEnumerable<AccountPermissionEntry> SeedPermissions()
    {
        return ScopePermissionsBuilder.Create("\\")
            .Account("acct_001", b => b.Allow(P_READ).Deny(P_WRITE))
            .Account("acct_002", b => b.Allow(P_READ).Allow(P_WRITE))
            .Scope("/organizations", s => s
                .Account("acct_001", b => b.Allow(P_READ).Deny(P_WRITE))
                .Account("acct_002", b => b.Allow(P_READ).Allow(P_WRITE))
            )
            .Scope("/organizations/projects/blues-brothers", s => s
                .Account("acct_001", b => b.Allow(P_READ).Deny(P_WRITE))
                .Account("acct_002", b => b.Allow(P_READ).Allow(P_WRITE))
            )
            .GetEntries();
    }

    public async Task DisposeAsync()
    {
        var containerA = new BlobContainerClient(ConnectionString, "objects");
        await containerA.DeleteIfExistsAsync();

        var containerB = new BlobContainerClient(ConnectionString, "permissions");
        await containerB.DeleteIfExistsAsync();
    }

    public async Task InitializeAsync()
    {
        await ObjectStore.UpsertAsync(_Objects);
        await PermissionStore.Permissions.SetRangeAsync(_Permissions);

        // Let's give Blob Storage time to index the tags
        await Task.Delay(2000);
    }

    private void CreateObject(string id, string name, string? searchable = null)
    {
        _Objects.Add(new BasicObject { Id = id, Name = name, SearchableString = searchable });
    }
}
