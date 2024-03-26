namespace Sparcpoint.Extensions.Permissions;

public sealed class InMemoryAccountPermissionStore : IAccountPermissionStore, IAccountPermissionChecker
{
    private readonly Dictionary<string, List<PermissionEntry>> _Entries;

    public InMemoryAccountPermissionStore()
    {
        _Entries = new(StringComparer.Ordinal);
    }

    public InMemoryAccountPermissionStore(IReadOnlyDictionary<string, IEnumerable<PermissionEntry>> entries)
    {
        _Entries = entries?.ToDictionary(kv => kv.Key, kv => kv.Value.ToList()) ?? new();
    }

    public IReadOnlyDictionary<string, IEnumerable<PermissionEntry>> Entries => _Entries.ToDictionary(kv => kv.Key, kv => (IEnumerable<PermissionEntry>)kv.Value);

    public Task<IEnumerable<PermissionEntry>> GetEntriesAsync(string accountId, ScopePath? scope = null)
    {
        Assertions.NotEmptyOrWhitespace(accountId);
        
        var entries = _Entries.TryGetValue(accountId, out List<PermissionEntry>? permissionCollection) ? permissionCollection : new List<PermissionEntry>();
        var result = entries.Where(e => e.Scope == (scope ?? ScopePath.RootScope)).ToArray();

        return Task.FromResult((IEnumerable<PermissionEntry>)result);
    }

    public Task<IEnumerable<ScopePath>> GetScopesAsync(string accountId)
    {
        Assertions.NotEmptyOrWhitespace(accountId);

        var entries = _Entries.TryGetValue(accountId, out List<PermissionEntry>? permissionCollection) ? permissionCollection : new List<PermissionEntry>();
        var result = entries.Select(e => e.Scope).Distinct().ToArray();
        return Task.FromResult((IEnumerable<ScopePath>)result);
    }

    public Task<bool?> IsAllowedAsync(string accountId, string key, ScopePath? scope = null)
    {
        Assertions.NotEmptyOrWhitespace(accountId);
        Assertions.NotEmptyOrWhitespace(key);

        if (!_Entries.TryGetValue(accountId, out List<PermissionEntry>? entries))
            return Task.FromResult((bool?)false);

        ScopePath realizedScope = scope ?? ScopePath.RootScope;
        while(true)
        {
            var found = entries.FirstOrDefault(e => e.Scope == realizedScope && e.Key == key);
            if (found != PermissionEntry.Empty)
                return Task.FromResult(found.IsAllowed);

            if (realizedScope == ScopePath.RootScope)
                break;

            realizedScope = realizedScope.Back();
        };

        return Task.FromResult((bool?)null);
    }

    public Task SetAsync(string accountId, PermissionEntry entry)
    {
        Assertions.NotEmptyOrWhitespace(accountId);
        Assertions.NotEmptyOrWhitespace(entry.Key);

        if (!_Entries.TryGetValue(accountId, out List<PermissionEntry>? permissionCollection))
        {
            permissionCollection = new List<PermissionEntry>();
            _Entries.Add(accountId, permissionCollection);
        }

        var foundEntries = permissionCollection.Where(e => e.Scope == entry.Scope && e.Key == entry.Key).ToArray();
        foreach (var found in foundEntries)
            permissionCollection.Remove(found);

        permissionCollection.Add(entry);

        return Task.CompletedTask;
    }
}