namespace Sparcpoint.Extensions.Objects;

public interface IObjectStoreFactory
{
    IObjectStore<T> CreateStore<T>() where T : class, ISparcpointObject;
    IObjectQuery<T> CreateQuery<T>() where T : class, ISparcpointObject;
}