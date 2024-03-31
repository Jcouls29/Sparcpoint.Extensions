using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Sparcpoint.Extensions.Objects;

namespace Sparcpoint.Extensions.Azure.Objects.BlobStorage;

internal class BlobStorageObjectQuery<T> : IObjectQuery<T> where T : class, ISparcpointObject
{
    private readonly BlobStorageObjectQuery _Query;
    public BlobStorageObjectQuery(BlobContainerClient client, BlobStorageObjectStoreOptions options)
    {
        _Query = new BlobStorageObjectQuery(client, options);
    }

    public async IAsyncEnumerable<T> RunAsync(ObjectQueryParameters parameters)
    {
        await foreach(var f in _Query.RunAsync(parameters with { WithType = typeof(T) }))
        {
            yield return (T)f;
        }
    }
}

internal class BlobStorageObjectQuery : IObjectQuery
{
    private readonly BlobContainerClient _Client;
    private readonly BlobStorageObjectStoreOptions _Options;

    public BlobStorageObjectQuery(BlobContainerClient client, BlobStorageObjectStoreOptions options)
    {
        _Client = client;
        _Options = options;
    }

    public async IAsyncEnumerable<ISparcpointObject> RunAsync(ObjectQueryParameters parameters)
    {
        // 1. Find the Id, if specified
        if (parameters.Id != null)
        {
            var found = await ByIdAsync(parameters);
            if (found != null)
                yield return found;

            yield break;
        }

        // 2. Set the tag search, if valid
        List<string> tagFilters = new();
        if (!string.IsNullOrWhiteSpace(parameters.Name))
            tagFilters.Add($"\"{Constants.NAME_KEY}\" = '{parameters.Name.EncodeBlobTagValue()}'");
        if (parameters.WithType != null)
            tagFilters.Add($"\"{Constants.TYPE_KEY}\" = '{parameters.WithType.AssemblyQualifiedName.EncodeBlobTagValue()}'");

        string? tagFilter = null;
        if (tagFilters.Count > 0)
            tagFilter = string.Join(" AND ", tagFilters);

        if (tagFilter != null)
        {
            await foreach (var f in FindByTagsAsync(tagFilter, parameters))
            {
                if (WithPropertiesMatch(f, parameters.WithProperties))
                    yield return f;
            }

            yield break;
        }

        string? prefix = null;
        if (parameters.ParentScope != null)
            prefix = parameters.ParentScope.ToString();
        
        await foreach(var f in FindByPrefixAsync(prefix, parameters))
        {
            if (WithPropertiesMatch(f, parameters.WithProperties))
                yield return f;
        }
    }

    private async Task<ISparcpointObject?> ByIdAsync(ObjectQueryParameters parameters)
    {
        var blobName = parameters.Id!.Value.Append(_Options.Filename);
        var bc = _Client.GetBlobClient(blobName);

        if (!await bc.ExistsAsync())
            return null;

        var tags = (await bc.GetTagsAsync())?.Value?.Tags;
        if (tags == null || tags.Count == 0 || !tags.TryGetValue(Constants.TYPE_KEY, out string? typeFullName))
            return null;

        Type? objType = Type.GetType(typeFullName.DecodeBlobTagValue());
        if (objType == null || !typeof(ISparcpointObject).IsAssignableFrom(objType))
            return null;

        if (parameters.WithType != null && parameters.WithType != objType)
            return null;

        var value = (ISparcpointObject?) await bc.GetAsJsonAsync(objType, skipExistenceCheck: true);
        if (value == null)
            return null;

        // Perform other checks based on parameters provided.
        if (!string.IsNullOrWhiteSpace(parameters.Name) && !parameters.Name.Equals(value.Name))
            return null;
        if (!string.IsNullOrWhiteSpace(parameters.NameStartsWith) && (value.Name == null || value.Name.StartsWith(parameters.NameStartsWith)))
            return null;
        if (!string.IsNullOrWhiteSpace(parameters.NameEndsWith) && (value.Name == null || value.Name.EndsWith(parameters.NameEndsWith)))
            return null;

        if (!WithPropertiesMatch(value, parameters.WithProperties))
            return null;

        return value;
    }

    private async IAsyncEnumerable<ISparcpointObject> FindByTagsAsync(string tagFilter, ObjectQueryParameters parameters)
    {
        List<Func<TaggedBlobItem, string, string, bool>> filters = new();
        filters.Add((item, typeName, name) => item.BlobContainerName == _Options.ContainerName);

        if (parameters.ParentScope != null)
            filters.Add((item, typeName, name) => (ScopePath.Parse(item.BlobName).Back(2) == parameters.ParentScope));

        if (parameters.WithType != null)
            filters.Add((item, typeName, name) => typeName.Equals(parameters.WithType.AssemblyQualifiedName));

        if (!string.IsNullOrWhiteSpace(parameters.NameStartsWith))
            filters.Add((item, typeName, name) => name.StartsWith(parameters.NameStartsWith));

        if (!string.IsNullOrWhiteSpace(parameters.NameEndsWith))
            filters.Add((item, typeName, name) => name.EndsWith(parameters.NameEndsWith));

        Func<TaggedBlobItem, string, string, bool> filter = (item, typeName, name) =>
        {
            foreach (var f in filters)
            {
                if (!f(item, typeName, name))
                    return false;
            }

            return true;
        };

        await foreach(var entry in _Client.FindBlobsByTagsAsync(tagFilter))
        {
            entry.Tags.TryGetValue(Constants.TYPE_KEY, out string? typeName);
            entry.Tags.TryGetValue(Constants.NAME_KEY, out string? name);

            if (typeName == null)
                continue;

            Type? type = Type.GetType(typeName.DecodeBlobTagValue());
            if (type == null)
                continue;

            if (name != null && filter(entry, typeName.DecodeBlobTagValue(), name.DecodeBlobTagValue()))
            {
                var bc = _Client.GetBlobClient(entry.BlobName);
                var value = await bc.GetAsJsonAsync(type, skipExistenceCheck: true);
                if (value != null)
                    yield return (ISparcpointObject)value;
            }
        }
    }

    private async IAsyncEnumerable<ISparcpointObject> FindByPrefixAsync(string? prefix, ObjectQueryParameters parameters)
    {
        List<Func<BlobItem, string, string, bool>> filters = new();

        if (parameters.ParentScope != null)
            filters.Add((item, typeName, name) => (ScopePath.Parse(item.Name).Back(2) == parameters.ParentScope));

        if (parameters.WithType != null)
            filters.Add((item, typeName, name) => typeName.Equals(parameters.WithType.AssemblyQualifiedName));

        if (!string.IsNullOrWhiteSpace(parameters.NameStartsWith))
            filters.Add((item, typeName, name) => name.StartsWith(parameters.NameStartsWith));

        if (!string.IsNullOrWhiteSpace(parameters.NameEndsWith))
            filters.Add((item, typeName, name) => name.EndsWith(parameters.NameEndsWith));

        Func<BlobItem, string, string, bool> filter = (item, typeName, name) =>
        {
            foreach (var f in filters)
            {
                if (!f(item, typeName, name))
                    return false;
            }

            return true;
        };

        await foreach(var entry in _Client.GetBlobsAsync(BlobTraits.Tags, prefix: prefix)) 
        {
            entry.Tags.TryGetValue(Constants.TYPE_KEY, out string? typeName);
            entry.Tags.TryGetValue(Constants.NAME_KEY, out string? name);

            if (typeName == null)
                continue;

            Type? type = Type.GetType(typeName.DecodeBlobTagValue());
            if (type == null)
                continue;

            if (name != null && filter(entry, typeName.DecodeBlobTagValue(), name.DecodeBlobTagValue()))
            {
                var bc = _Client.GetBlobClient(entry.Name);
                var value = await bc.GetAsJsonAsync(type, skipExistenceCheck: true);
                if (value != null)
                    yield return (ISparcpointObject)value;
            }
        }
    }

    private bool WithPropertiesMatch(ISparcpointObject value, Dictionary<string, string>? withProperties)
    {
        if (withProperties == null || withProperties.Count == 0)
            return true;

        var valueProps = value.GetProperties();
        foreach (var prop in withProperties)
        {
            if (!valueProps.TryGetValue(prop.Key, out string? foundPropValue))
                return false;

            if (foundPropValue == null && prop.Value == null)
                continue;

            if (foundPropValue == null || prop.Value == null)
                return false;

            if (!foundPropValue.Equals(prop.Value))
                return false;
        }

        return true;
    }
}