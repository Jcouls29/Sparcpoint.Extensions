namespace Sparcpoint.Extensions.Resources;

public interface IResourceDataClient<T> where T : class, new()
{
    ScopePath ResourceId { get; }

    Task<T?> GetAsync();
    Task DeleteAsync();
    Task SaveAsync(T data);

    Task<ResourcePermissions?> GetPermissionsAsync();
    Task SetPermissionsAsync(ResourcePermissions permissions);
}
