﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Azure;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

        ETag? originalTag = null;
        if (await client.ExistsAsync())
        {
            var props = await client.GetPropertiesAsync();
            originalTag = props.Value.ETag;
        }

        BlobUploadOptions? blobOptions = null;
        if (tags != null && tags.Count > 0)
        {
            blobOptions = new BlobUploadOptions
            {
                Tags = tags,
                Conditions = new BlobRequestConditions
                {
                    IfMatch = originalTag,
                    IfNoneMatch = originalTag == null ? ETag.All : null,
                },
                
            };
        }

        using (var stream = new MemoryStream())
        {
            JsonSerializer.Serialize(stream, value, options);
            stream.Position = 0;
            await client.UploadAsync(stream, blobOptions);
        }
    }

    public static async Task UpdateAsJsonAsync<T>(this BlobClient client, Func<T?, Task<T?>> updater, JsonSerializerOptions? options = default, IDictionary<string, string>? tags = null)
    {
        T? updatedValue = default;
        ETag? originalETag = null;

        IDictionary<string, string>? optionTags = null;
        if (tags != null && tags.Count > 0)
            optionTags = tags;

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

            var writeOptions = new BlobOpenWriteOptions
            {
                OpenConditions = new BlobRequestConditions { IfMatch = originalETag },
                Tags = optionTags
            };
            using (var writeStream = await client.OpenWriteAsync(true, writeOptions))
            {
                JsonSerializer.Serialize(writeStream, updatedValue, options);
                await writeStream.FlushAsync();
            }
        }
        else
        {
            updatedValue = await updater(default);
            
            // If we're still null then we do not need to do anything
            if (updatedValue == null)
                return;

            var writeOptions = new BlobUploadOptions
            {
                Tags = optionTags,
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
            };

            using (var stream = new MemoryStream())
            {
                JsonSerializer.Serialize(stream, updatedValue, options);
                stream.Position = 0;
                await client.UploadAsync(stream, writeOptions);
            }
        }
    }

    [return: NotNullIfNotNull(nameof(value))]
    public static string? EncodeBlobTagValue(this string? value)
    {
        if (value == null)
            return null;

        StringBuilder builder = new StringBuilder();

        foreach(var c in value)
        {
            if (IsValidBlobTagCharacter(c))
            {
                builder.Append(c);
                continue;
            }

            string replace = "/:" + (int)c + ":/";
            builder.Append(replace);
        }

        var result = builder.ToString();
        if (!IsValidBlobTagValue(result, out string? message))
            throw new InvalidOperationException($"Could not sanitize tag value: {message}");

        return result;
    }

    private static Regex Pattern = new Regex("/:(\\d+):/");
    [return: NotNullIfNotNull(nameof(value))]
    public static string? DecodeBlobTagValue(this string? value)
    {
        if (value == null)
            return null;

        return Pattern.Replace(value, (Match m) =>
        {
            var c = (char)(int.Parse(m.Groups[1].Value));
            return c.ToString();
        });
    }

    public static bool IsValidBlobTagValue(this string value, out string? message)
    {
        message = null;

        if (value == null)
            return true;
        if (value.Length > 256)
        {
            message = "Tag value cannot be longer than 256 characters.";
            return false;
        }

        foreach(var c in value)
        {
            if (!IsValidBlobTagCharacter(c))
            {
                message = $"'{value}' has invalid character: {c}";
                return false;
            }
        }

        return true;
    }

    private static bool IsValidBlobTagCharacter(char c)
        => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || (c == ' ' || c == '+' || c == '-' || c == '.' || c == ':' || c == '=' || c == '_' || c == '/');
}