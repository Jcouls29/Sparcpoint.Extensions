using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sparcpoint.Common.Initializers;
using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Azure.Permissions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStoragePermissions(this IServiceCollection services, BlobStoragePermissionStoreOptions options)
    {
        Ensure.ArgumentNotNull(options);
        Ensure.NotNullOrWhiteSpace(options.ConnectionString);
        Ensure.NotNullOrWhiteSpace(options.ContainerName);
        Ensure.NotNullOrWhiteSpace(options.Filename);
        // TODO: Add better validation for Container Name / Filename

        BlobContainerClient client = new BlobContainerClient(options.ConnectionString, options.ContainerName);

        services.TryAddSingleton<IPermissionStore>(new BlobStoragePermissionStore(client, options));
        services.TryAddSingleton<IAccountPermissionQuery>(new BlobStorageAccountPermissionQuery(client, options.Filename));
        services.TryAddSingleton<IInitializerRunner, DefaultInitializerRunner>();
        services.AddSingleton<IInitializer>(new EnsureContainerCreatedInitializer(client));

        return services;
    }
}
