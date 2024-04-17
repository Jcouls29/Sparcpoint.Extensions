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

    public static async IAsyncEnumerable<SparcpointResourceDisplayEntry> ListChildrenAsync<TChild>(this IResourceDataClient parent, Func<TChild, string> displayValueFunc, int maxDepth = 2)
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
}
