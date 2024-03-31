using Azure.Storage.Blobs;
using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Azure.Permissions;

internal class BlobStoragePermissionStore : IPermissionStore
{
    public const string PERMISSION_FILE_NAME = "permissions";
    private readonly BlobContainerClient _Client;
    private readonly BlobStoragePermissionStoreOptions _Options;

    public BlobStoragePermissionStore(BlobContainerClient client, BlobStoragePermissionStoreOptions options)
    {
        _Client = client ?? throw new ArgumentNullException(nameof(client));
        _Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public IAccountPermissionCollection Get(string accountId, ScopePath? scope = null)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        return new BlobStorageAccountPermissionCollection(_Client, accountId, scope);
    }

    public IScopePermissionCollection Get(ScopePath scope)
        => new BlobStorageScopePermissionCollection(_Client, scope);

    public IAccountPermissionView GetView(string accountId, ScopePath? scope = null)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        return new BlobStorageAccountPermissionView(_Client, accountId, scope, _Options.View ?? BlobStoragePermissionStoreViewOptions.Default);
    }

    public IScopePermissionView GetView(ScopePath scope)
        => new BlobStorageScopePermissionView(_Client, scope, _Options.View ?? BlobStoragePermissionStoreViewOptions.Default);
}
