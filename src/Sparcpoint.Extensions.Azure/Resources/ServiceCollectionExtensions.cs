using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Sparcpoint.Extensions.Azure.Resources.BlobStorage;
using Sparcpoint.Extensions.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Azure.Resources;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStorageResources(this IServiceCollection services, BlobStorageResourceStoreOptions options)
    {
        Ensure.ArgumentNotNull(options);
        Ensure.NotNullOrWhiteSpace(options.ConnectionString);
        Ensure.NotNullOrWhiteSpace(options.ContainerName);

        return services.AddSingleton<IResourceStore>(new BlobStorageResourceStore(new BlobContainerClient(options.ConnectionString, options.ContainerName)));
    }
}
