using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System.Text.Json;

namespace Azure.Data.Tables;

public static class BlobClientExtensions
{
    public static async Task EnsureContainerCreatedAsync(this BlobClient client)
    {
        var containerClient = client.GetParentBlobContainerClient();
        await containerClient.CreateIfNotExistsAsync();
    }

    public static async Task<T?> GetAsJsonAsync<T>(this BlobClient client, JsonSerializerOptions? options = default) where T : class
    {
        if (!await client.ExistsAsync())
            return null;

        var response = await client.DownloadContentAsync();
        var contents = response.Value.Content.ToString();
        return JsonSerializer.Deserialize<T>(contents);
    }

    public static async Task UpdateAsJsonAsync<T>(this BlobClient client, T? value, JsonSerializerOptions? options = default) where T : class
    {
        if (value == null)
        {
            await client.DeleteIfExistsAsync();
            return;
        }

        using (var writeStream = await client.OpenWriteAsync(true))
        {
            JsonSerializer.Serialize(writeStream, value, options);
            await writeStream.FlushAsync();
        }
    }

    public static async Task UpdateAsJsonAsync<T>(this BlobClient client, Func<T?, Task<T?>> updater, JsonSerializerOptions? options = default)
    {
        T? updatedValue = default;
        ETag originalETag = ETag.All;

        if (await client.ExistsAsync())
        {
            var response = await client.DownloadContentAsync();
            var contents = response.Value.Content.ToString();
            T? originalValue = JsonSerializer.Deserialize<T>(contents);
            originalETag = response.Value.Details.ETag;
            updatedValue = await updater(originalValue);

            if (updatedValue == null)
            {
                // If we're null after the updater, delete the blob
                await client.DeleteIfExistsAsync();
                return;
            }
        } 
        else
        {
            updatedValue = await updater(default);
            
            // If we're still null then we do not need to do anything
            if (updatedValue == null)
                return;
        }

        var writeOptions = new Storage.Blobs.Models.BlobOpenWriteOptions { OpenConditions = new Storage.Blobs.Models.BlobRequestConditions { IfMatch = originalETag } };
        using (var writeStream = await client.OpenWriteAsync(true, writeOptions))
        {
            JsonSerializer.Serialize(writeStream, updatedValue, options);
            await writeStream.FlushAsync();
        }
    }
}
