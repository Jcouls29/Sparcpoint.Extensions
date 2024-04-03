using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Sparcpoint.Extensions.Permissions;
using Sparcpoint.Extensions.Permissions.Extensions;

namespace Sparcpoint.Extensions.Azure.Permissions;

internal class BlobStorageAccountPermissionView : IAccountPermissionView
{
    private readonly BlobContainerClient _Client;
    private readonly string _Filename;
    private readonly BlobStoragePermissionStoreViewOptions _Options;

    public BlobStorageAccountPermissionView(BlobContainerClient client, string accountId, ScopePath? scope, BlobStoragePermissionStoreViewOptions options, string filename)
    {
        _Client = client;
        AccountId = accountId;
        CurrentScope = scope ?? ScopePath.RootScope;
        _Filename = filename;
        _Options = options;
    }

    public string AccountId { get; }

    public ScopePath CurrentScope { get; }

    public async IAsyncEnumerator<PermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await _Client.EnsureCreatedAsync();

        // 1. Load up all Entries
        List<AccountPermissionEntry> entries = new();
        await foreach (var scope in GetValidScopes())
        {
            var bc = _Client.GetBlobClient(scope.Append(_Filename));

            var values = await bc.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
            if (values != null)
                entries.AddRange(values.Where(e => e.AccountId == AccountId).Select(c => new AccountPermissionEntry(c.AccountId, CurrentScope, new PermissionEntry(c.Key, c.Value, c.Metadata))));
        }

        // 2. Calculate View
        var view = entries.CalculateView(CurrentScope);
        foreach (var e in view)
            yield return e.Entry;
    }

    private async IAsyncEnumerable<ScopePath> GetValidScopes()
    {
        var hierarchy = CurrentScope.GetHierarchy(includeRootScope: _Options.IncludeRootScopeInCalculations);
        if (CurrentScope == ScopePath.RootScope)
        {
            yield return ScopePath.RootScope;
            yield break;
        }

        foreach (var entry in hierarchy)
        {
            var bc = _Client.GetBlobClient(entry.Append(_Filename));
            if (await bc.ExistsAsync())
                yield return entry;
        }
    }
}