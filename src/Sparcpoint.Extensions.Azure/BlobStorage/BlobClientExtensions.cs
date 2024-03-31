using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System.Text.Json;

namespace Azure.Data.Tables;

public static class BlobClientExtensions
{
    public static async Task<T?> GetAsJsonAsync<T>(this BlobClient client, JsonSerializerOptions? options = default) where T : class
        => (T?) await GetAsJsonAsync(client, typeof(T), options);

    public static async Task<object?> GetAsJsonAsync(this BlobClient client, Type type, JsonSerializerOptions? options = default, bool skipExistenceCheck = false)
    {
        if (!skipExistenceCheck && !await client.ExistsAsync())
            return null;
        
        var response = await client.DownloadContentAsync();
        var contents = response.Value.Content.ToString();
        return JsonSerializer.Deserialize(contents, type, options);
    }

    public static async Task UpdateAsJsonAsync<T>(this BlobClient client, T? value, JsonSerializerOptions? options = default, IDictionary<string, string>? tags = null) where T : class
    {
        if (value == null)
        {
            await client.DeleteIfExistsAsync();
            return;
        }

        BlobOpenWriteOptions? blobOptions = null;
        if (tags != null && tags.Count > 0)
        {
            blobOptions = new BlobOpenWriteOptions
            {
                Tags = tags
            };
        }

        using (var writeStream = await client.OpenWriteAsync(true, options: blobOptions))
        {
            JsonSerializer.Serialize(writeStream, value, options);
            await writeStream.FlushAsync();
        }
    }

    public static async Task UpdateAsJsonAsync<T>(this BlobClient client, Func<T?, Task<T?>> updater, JsonSerializerOptions? options = default, IDictionary<string, string>? tags = null)
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

        IDictionary<string, string>? optionTags = null;
        if (tags != null && tags.Count > 0)
            optionTags = tags;

        var writeOptions = new Storage.Blobs.Models.BlobOpenWriteOptions 
        { 
            OpenConditions = new Storage.Blobs.Models.BlobRequestConditions { IfMatch = originalETag },
            Tags = optionTags
        };
        using (var writeStream = await client.OpenWriteAsync(true, writeOptions))
        {
            JsonSerializer.Serialize(writeStream, updatedValue, options);
            await writeStream.FlushAsync();
        }
    }
}
