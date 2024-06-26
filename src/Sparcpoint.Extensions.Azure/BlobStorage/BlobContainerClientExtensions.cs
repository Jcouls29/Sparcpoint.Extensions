﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using SmartFormat;
using Sparcpoint.Extensions.Azure;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Azure.Data.Tables;

public static class BlobContainerClientExtensions
{
    public static async Task EnsureContainerCreatedAsync(this BlobClient client)
    {
        var containerClient = client.GetParentBlobContainerClient();
        await containerClient.CreateIfNotExistsAsync();
    }

    public static async Task EnsureCreatedAsync(this BlobContainerClient client)
    {
        const int RETRY_COUNT = 3;

        for (int i = 0; i < RETRY_COUNT; i++)
        {
            try
            {
                await client.CreateIfNotExistsAsync();
                return;
            } catch
            {
                await Task.Delay(250);
            }
        }

        await client.CreateIfNotExistsAsync();
    }

    public static async Task BulkDeleteAsync(this BlobContainerClient client, IEnumerable<string> blobNames)
    {
        // TODO: Atomicity
        foreach (var blobName in blobNames)
        {
            await client.DeleteBlobIfExistsAsync(blobName);
        }
    }

    public static async Task<BlobContainerClient> GetContainer(this BlobServiceClient service, Type entityType, params object?[] parameters)
    {
        var attr = GetKeyAttribute(entityType);
        string containerName = attr.ContainerName;
        foreach(var p in parameters)
        {
            containerName = Smart.Format(containerName, p);
        }

        containerName = containerName.ToLower();

        if (string.IsNullOrWhiteSpace(containerName))
            throw new InvalidOperationException("Container Key is required on BlobKeyAttribute");

        var client = service.GetBlobContainerClient(containerName);
        if (!await client.ExistsAsync())
        {
            await client.CreateIfNotExistsAsync();
        }

        return client;
    }

    public static async Task<bool> DeleteAsync<T>(this BlobServiceClient service, object? parameters = null)
    {
        return await WithClient(service, typeof(T), async (blob) => await blob.DeleteIfExistsAsync(), parameters);
    }

    public static async Task<bool> ExistsAsync<T>(this BlobServiceClient service, object? parameters = null)
    {
        return await WithClient(service, typeof(T), async (blob) => await blob.ExistsAsync(), parameters);
    }

    public static async Task UpsertAsync<T>(this BlobServiceClient service, T entity, object? parameters = null)
    {
        await WithClient(service, typeof(T), async (blob) =>
        {
            var json = JsonSerializer.Serialize(entity, Options);

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(json);
                await writer.FlushAsync();
                stream.Position = 0;

                await blob.UploadAsync(stream, overwrite: true);
            }

            // No Return Required
            return 0;
        }, entity, parameters);
    }

    public static async Task<T?> FindAsync<T>(this BlobServiceClient service, object? parameters = null)
    {
        return await WithClient(service, typeof(T), async (blob) =>
        {
            if (!await blob.ExistsAsync())
                return default;

            var response = await blob.DownloadStreamingAsync();
            using (var stream = response.Value.Content)
            using (var reader = new StreamReader(stream))
            {
                var json = await reader.ReadToEndAsync();
                return JsonSerializer.Deserialize<T>(json, Options);
            }
        }, parameters);
    }

    private static async Task<TReturnType> WithClient<TReturnType>(this BlobServiceClient service, Type entityType, Func<BlobClient, Task<TReturnType>> action, params object?[] parameters)
    {
        var client = await GetContainer(service, entityType, parameters);

        var attr = GetKeyAttribute(entityType);

        string blobName = attr.PathFormat;
        foreach(var p in parameters)
        {
            if (p == null)
                continue;

            blobName = Smart.Format(blobName, p);
        }

        var blob = client.GetBlobClient(blobName);
        return await action(blob);
    }

    private static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private static BlobKeyAttribute GetKeyAttribute(Type entityType)
    {
        var attr = entityType.GetCustomAttribute<BlobKeyAttribute>();
        if (attr == null)
            throw new InvalidOperationException("This operation requires the entity to be decorated with a BlobKeyAttribute.");

        if (string.IsNullOrWhiteSpace(attr.PathFormat))
            throw new InvalidOperationException("PathFormat is empty.");
        if (string.IsNullOrWhiteSpace(attr.ContainerName))
            throw new InvalidOperationException("ContainerName is empty.");

        return attr;
    }
}
