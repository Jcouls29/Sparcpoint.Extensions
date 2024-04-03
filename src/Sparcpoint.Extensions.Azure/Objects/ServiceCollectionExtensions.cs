using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sparcpoint.Common.Initializers;
using Sparcpoint.Extensions.Azure.Objects.BlobStorage;
using Sparcpoint.Extensions.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Azure.Objects;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStorageObjects(this IServiceCollection services, BlobStorageObjectStoreOptions options)
    {
        Ensure.NotNull(options);
        Ensure.ArgumentNotNullOrWhiteSpace(options.ConnectionString);
        Ensure.ArgumentNotNullOrWhiteSpace(options.ContainerName);
        Ensure.ArgumentNotNullOrWhiteSpace(options.Filename);

        var containerClient = new BlobContainerClient(options.ConnectionString, options.ContainerName);

        services.TryAddSingleton<IInitializerRunner, DefaultInitializerRunner>();
        services.AddSingleton<IInitializer>(new EnsureContainerCreatedInitializer(containerClient));
        services.TryAddSingleton(typeof(IObjectStore<>), typeof(FactoryObjectStoreWrapper<>));
        services.TryAddSingleton(typeof(IObjectQuery<>), typeof(FactoryObjectQueryWrapper<>));
        services.WithChildServices<IObjectStoreFactory, BlobStorageObjectStoreFactory>(child =>
        {
            child.AddSingleton(containerClient);
            child.AddSingleton(options);
        });
        services.WithChildServices<IObjectQuery, BlobStorageObjectQuery>(child =>
        {
            child.AddSingleton(containerClient);
            child.AddSingleton(options);
        });
        services.WithChildServices<IObjectIdNameQuery, BlobStorageObjectIdNameQuery>(child =>
        {
            child.AddSingleton(containerClient);
            child.AddSingleton(options);
        });

        return services;
    }
}
