namespace Sparcpoint.Extensions.Objects;

public sealed class FactoryObjectStoreWrapper<T> : IObjectStore<T> where T : class, ISparcpointObject
{
    private readonly IObjectStore<T> _InnerService;

    public FactoryObjectStoreWrapper(IObjectStoreFactory factory)
    {
        _InnerService = factory.CreateStore<T>();
    }

    public async Task DeleteAsync(IEnumerable<ScopePath> ids)
        => await _InnerService.DeleteAsync(ids);

    public async Task<T?> FindAsync(ScopePath id)
        => await _InnerService.FindAsync(id);

    public async Task UpsertAsync(IEnumerable<T> o)
        => await _InnerService.UpsertAsync(o);
}
