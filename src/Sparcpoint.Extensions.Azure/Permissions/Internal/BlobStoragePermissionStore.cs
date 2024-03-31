using Azure.Storage.Blobs;
using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Azure.Permissions;

// NOTE: This is a naive implementation of permission storage in blob containers.
//       Instead, a better approach would be to create a global index, store
//       permissions in Page Blobs (rather than block blobs) and randomly access
//       said permissions directly. This other approach has the potential to 
//       be much faster for look ups versus simply loading up all the permissions
//       from files each time.
internal class BlobStoragePermissionStore : IPermissionStore
{
    private readonly BlobContainerClient _Client;
    private readonly BlobStoragePermissionStoreOptions _Options;

    public BlobStoragePermissionStore(BlobContainerClient client, BlobStoragePermissionStoreOptions options)
    {
        _Client = client ?? throw new ArgumentNullException(nameof(client));
        _Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public IAccountPermissionCollection Get(string accountId, ScopePath? scope = null)
    {
        _Client.CreateIfNotExists();

        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        return new BlobStorageAccountPermissionCollection(_Client, accountId, scope, _Options.Filename);
    }

    public IScopePermissionCollection Get(ScopePath scope)
    {
        _Client.CreateIfNotExists();
        return new BlobStorageScopePermissionCollection(_Client, scope, _Options.Filename);
    }
        

    public IAccountPermissionView GetView(string accountId, ScopePath? scope = null)
    {
        _Client.CreateIfNotExists();

        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        return new BlobStorageAccountPermissionView(_Client, accountId, scope, _Options.View ?? BlobStoragePermissionStoreViewOptions.Default, _Options.Filename);
    }

    public IScopePermissionView GetView(ScopePath scope)
    {
        _Client.CreateIfNotExists();
        return new BlobStorageScopePermissionView(_Client, scope, _Options.View ?? BlobStoragePermissionStoreViewOptions.Default, _Options.Filename);
    }
}
