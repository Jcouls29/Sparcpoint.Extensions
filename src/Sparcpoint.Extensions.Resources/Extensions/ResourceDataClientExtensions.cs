﻿using SmartFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Resources;

public static class ResourceDataClientExtensions
{
    public static async Task<IResourceDataClient<TChild>> SaveAsync<TChild>(this IResourceDataClient parent, ScopePath childRelativePath, TChild childData, ResourcePermissions? permissions = null)
        where TChild : class, new()
    {
        var client = parent.GetChildClient<TChild>(childRelativePath);
        await client.SaveAsync(childData);

        if (permissions != null)
            await client.SetPermissionsAsync(permissions);

        return client;
    }

    public static async Task<IResourceDataClient<TChild>> SaveAsync<TChild>(this IResourceDataClient parent, TChild childData, ResourcePermissions? permissions = null)
        where TChild : class, new()
    {
        var format = ResourceIdAttribute.GetFormat(typeof(TChild));
        if (format == null)
            throw new InvalidOperationException($"A `ResourceIdAttribute` with `Format` specified is required to utilize this save method. [Type = {typeof(TChild).Name}]");

        var childRelativePath = Smart.Format(format, childData);
        if (childRelativePath == null)
            throw new InvalidOperationException($"Could not properly format the child's resource path. [Format = {format}, Type = {typeof(TChild).Name}]");
        if (childRelativePath.Contains("{") || childRelativePath.Contains("}"))
            throw new InvalidOperationException($"Brackets cannot be leftover after formatting of the resource id. Please check that all properties exist on the object. [Format = {format}, Type = {typeof(TChild).Name}]");

        var client = parent.GetChildClient<TChild>(childRelativePath);
        await client.SaveAsync(childData);

        if (permissions != null)
            await client.SetPermissionsAsync(permissions);

        return client;
    }

    public static async IAsyncEnumerable<TChild> GetChildrenAsync<TChild>(this IResourceDataClient parent, int maxDepth = int.MaxValue) where TChild : class, new()
    {
        await foreach(var item in parent.GetChildClientsAsync<TChild>(maxDepth))
        {
            var data = await item.GetAsync();
            if (data != null)
                yield return data;
        }
    }

    public static async IAsyncEnumerable<SparcpointResourceDisplayEntry> ListChildrenAsync<TChild>(this IResourceDataClient parent, Func<TChild, string> displayValueFunc, int maxDepth = int.MaxValue)
        where TChild : class, new()
    {
        var resourceType = ResourceTypeAttribute.GetResourceType<TChild>();
        await foreach(var item in parent.GetChildClientsAsync<TChild>(maxDepth))
        {
            var data = await item.GetAsync();
            if (data == null)
                continue;

            var permissions = await item.GetPermissionsAsync();
            var displayValue = displayValueFunc(data);

            yield return new SparcpointResourceDisplayEntry(item.ResourceId, resourceType, permissions, displayValue);
        }
    }

    public static async IAsyncEnumerable<SparcpointResourceDisplayEntry> ListChildrenAsync<TChild>(this IResourceDataClient parent, Func<TChild, string> displayValueFunc, ScopePath childPrefix) where TChild : class, new()
    {
        var resourceType = ResourceTypeAttribute.GetResourceType<TChild>();
        await foreach (var item in parent.GetChildClientsAsync<TChild>(childPrefix))
        {
            var data = await item.GetAsync();
            if (data == null)
                continue;

            var permissions = await item.GetPermissionsAsync();
            var displayValue = displayValueFunc(data);

            yield return new SparcpointResourceDisplayEntry(item.ResourceId, resourceType, permissions, displayValue);
        }
    }

    public static async IAsyncEnumerable<IResourceDataClient<TChild>> GetChildClientsAsync<TChild>(this IResourceDataClient parent, ScopePath childPrefix) where TChild : class, new()
    {
        var prefix = parent.ResourceId + childPrefix;
        await foreach(var item in parent.GetChildClientsAsync<TChild>(maxDepth: int.MaxValue))
        {
            if (item.ResourceId.StartsWith(prefix))
                yield return item;
        }
    }
}
