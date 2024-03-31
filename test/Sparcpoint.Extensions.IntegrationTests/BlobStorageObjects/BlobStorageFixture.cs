using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Sparcpoint.Extensions.IntegrationTests;

public class BlobStorageFixture : IAsyncLifetime
{
    public IObjectStore<BasicObject> ObjectStore { get; }
    public IObjectQuery<BasicObject> ObjectQuery { get; }

    public BlobStorageFixture()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        var connectionString = config["BlobStorage:ConnectionString"];
        var containerName = "objects";
        var filename = ".object";

        var services = new ServiceCollection();
        services.AddBlobStorageObjects(new Azure.Objects.BlobStorage.BlobStorageObjectStoreOptions { ConnectionString = connectionString, ContainerName = containerName, Filename = filename });
        var provider = services.BuildServiceProvider();

        ObjectStore = provider.GetRequiredService<IObjectStore<BasicObject>>();
        ObjectQuery = provider.GetRequiredService<IObjectQuery<BasicObject>>();

        SeedServices();
    }

    private readonly List<BasicObject> _Objects = new();
    private void SeedServices()
    {
        CreateObject("/organizations/org_01", "Organization 01", "My Organization");
        CreateObject("/organizations/org_01/projects/prj_01", "Project 01", "My Project 01");
        CreateObject("/organizations/org_01/projects/prj_02", "Project 02", "My Project 02");
        CreateObject("/organizations/org_01/projects/prj_03", "Project 03", "My Project 03");

        CreateObject("/organizations/org_02", "Organization 02", "My Organization 2");
        CreateObject("/organizations/org_02/projects/prj_01", "Project 04", "My Project 04");

        CreateObject("/organizations/org_03", "Organization 03", "Other Organization");
        CreateObject("/organizations/org_03/projects/prj_01", "Project 01", "Other Project 01");

        CreateObject("/organizations/org_04", "Organization 04", "Other Organization 2");

        CreateObject("/rules/rule_01", "Rule 01", null);
    }

    public async Task DisposeAsync()
    {
        await ObjectStore.DeleteAsync(_Objects.Select(o => o.Id));
    }

    public async Task InitializeAsync()
    {
        await ObjectStore.UpsertAsync(_Objects);
    }

    private void CreateObject(string id, string name, string? searchable = null)
    {
        _Objects.Add(new BasicObject { Id = id, Name = name, SearchableString = searchable });
    }
}
