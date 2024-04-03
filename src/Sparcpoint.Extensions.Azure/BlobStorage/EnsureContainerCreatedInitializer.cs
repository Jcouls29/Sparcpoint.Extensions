using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Sparcpoint.Common.Initializers;

namespace Sparcpoint.Extensions.Azure;

internal class EnsureContainerCreatedInitializer : IInitializer
{
    private readonly BlobContainerClient _Client;

    public EnsureContainerCreatedInitializer(BlobContainerClient client)
    {
        _Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task InitializeAsync()
    {
        // TODO: Add Options to wait for completion
        await _Client.EnsureCreatedAsync();
    }
}