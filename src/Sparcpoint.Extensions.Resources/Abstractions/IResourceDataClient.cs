namespace Sparcpoint.Extensions.Resources;

public interface IResourceDataClient<T> where T : class, new()
{
    ScopePath ResourceId { get; }

    Task<T?> GetAsync();
    Task DeleteAsync();
    Task SaveAsync(T data);

    Task<ResourcePermissions?> GetPermissionsAsync();
    Task SetPermissionsAsync(ResourcePermissions permissions);

    IAsyncEnumerable<IResourceDataClient<TChild>> GetChildClientsAsync<TChild>(int maxDepth = 2) where TChild : class, new();
    IResourceDataClient<TChild> GetChildClient<TChild>(ScopePath relativePath) where TChild : class, new();
}
