﻿using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Azure.Permissions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStoragePermissions(this IServiceCollection services, BlobStoragePermissionStoreOptions options)
    {
        Ensure.ArgumentNotNull(options);
        Ensure.NotNullOrWhiteSpace(options.ConnectionString);
        Ensure.NotNullOrWhiteSpace(options.ContainerName);

        BlobContainerClient client = new BlobContainerClient(options.ConnectionString, options.ContainerName);
        services.TryAddSingleton<IPermissionStore>(new BlobStoragePermissionStore(client, options));

        return services;
    }
}
