namespace Sparcpoint.Extensions.Resources;

public interface IResourceDataClientFactory
{
    IResourceDataClient<T> Create<T>(ScopePath resourceId) where T : class, new();
}
