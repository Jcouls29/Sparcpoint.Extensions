namespace Sparcpoint.Extensions.Resources;

public interface IResourceDataClient
{
    ScopePath ResourceId { get; }
    Task DeleteAsync();

    Task<ResourcePermissions> GetPermissionsAsync();
    Task SetPermissionsAsync(ResourcePermissions permissions);

    IAsyncEnumerable<IResourceDataClient<TChild>> GetChildClientsAsync<TChild>(int maxDepth = int.MaxValue) where TChild : class, new();
    IResourceDataClient<TChild> GetChildClient<TChild>(ScopePath relativePath) where TChild : class, new();
}

public interface IResourceDataClient<T> : IResourceDataClient
    where T : class, new()
{
    Task<T?> GetAsync();
    Task SaveAsync(T data);
}
