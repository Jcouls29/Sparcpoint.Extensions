using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Resources;

public interface ISparcpointResource
{
    string Name { get; }
}

public sealed class SparcpointSubscription
{
    public string Name { get; set; }
}

public sealed class SparcpointOrganization
{
    public string Name { get; set; }
}

public sealed class SparcpointProvider
{
    public string Name { get; set; }
}

public interface ISparcpointClient
{
    IAsyncEnumerable<ISubscriptionClient> GetSubscriptionsAsync();
    Task<ISubscriptionClient> GetSubscriptionAsync(string subscriptionId);
    IScopePermissionView GetAccessControlList();

    Task<ISubscriptionClient> CreateAsync(SparcpointSubscription subscription);
}

public interface ISubscriptionClient
{
    public ScopePath Id { get; }

    Task<SparcpointSubscription> GetPropertiesAsync();
    IScopePermissionView GetAccessControlList();

    IAsyncEnumerable<IOrganizationClient> GetOrganizationsAsync();
    IOrganizationClient GetOrganizationClient(string organizationId);

    Task<IOrganizationClient> CreateAsync(SparcpointOrganization organization);
}

public interface IOrganizationClient
{
    public ScopePath Id { get; }

    Task<SparcpointOrganization> GetPropertiesAsync();
    IScopePermissionView GetAccessControlList();

    IAsyncEnumerable<IProviderClient> GetProvidersAsync();
    IProviderClient GetProviderClient(string providerName);

    Task<IProviderClient> CreateAsync(SparcpointProvider provider);
}

public interface IProviderClient
{
    public ScopePath Id { get; }

    Task<SparcpointProvider> GetPropertiesAsync();
    IScopePermissionView GetAccessControlList();

    // TODO: Need to think more on this...
    IAsyncEnumerable<IResourceClient<TProp>> GetResourceClientsAsync<TProp>() where TProp : class, new();
    IResourceClient<TProp> GetResourceClient<TProp>() where TProp : class, new();
}

public interface IResourceClient<TProperties> where TProperties : class, new()
{
    Task SetProperties(ResourceId resourceId, TProperties properties);
    Task<TProperties> GetProperties(ResourceId resourceId);
    IScopePermissionView GetAccessControlList(ResourceId resourceId);
}

public abstract class ResourceClient<TProperties> where TProperties : class, new()
{
    private readonly IResourceClient<TProperties> _Client;
    private readonly ScopePath _ResourceId;
    private readonly ScopePath? _ParentResourceId;

    public ResourceClient(IResourceClient<TProperties> client, ResourceId resourceId)
    {
        Ensure.ArgumentNotNull(client);
        Ensure.ArgumentNotNullOrWhiteSpace(resourceId);

        _Client = client;
        _ResourceId = resourceId;
        _ParentResourceId = parentResourceId;
    }

    

    public async Task<TProperties> GetProperties()
        => await _Client.GetProperties()
}

public sealed class SubscriptionClient
{
    private readonly ScopePath PREFIX = "/subscription";
    private readonly string _Name;

    public SubscriptionClient(string subscriptionName)
    {
        _Name = subscriptionName;
    }

    public ScopePath Id => PREFIX + _Name;

    public SparcpointSubscription GetProperties()
        => new SparcpointSubscription();

    public IEnumerable<AccountPermissionEntry> GetAccessControlList()
        => Array.Empty<AccountPermissionEntry>();

    public OrganizationClient[] GetOrganizations()
        => Array.Empty<OrganizationClient>();
}

public sealed class OrganizationClient
{
    private readonly ScopePath PREFIX = "/organization";
    private readonly ScopePath _Parent;
    private readonly string _Name;

    public OrganizationClient(ScopePath parent, string name)
    {
        _Parent = parent;
        _Name = name;
    }

    public ScopePath Id => _Parent + PREFIX + _Name;

    public SparcpointOrganization GetProperties()
        => new SparcpointOrganization();

    public IEnumerable<AccountPermissionEntry> GetAccessControlList()
        => Array.Empty<AccountPermissionEntry>();

    // ... Get Applications
}