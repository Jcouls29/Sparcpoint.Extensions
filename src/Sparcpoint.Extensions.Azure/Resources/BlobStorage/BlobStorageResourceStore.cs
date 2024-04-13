using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Sparcpoint.Extensions.Permissions;
using Sparcpoint.Extensions.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Azure.Resources.BlobStorage
{
    // TODO: MUST IMPLEMENT PERMISSION-CHECKER INTERFACE TO ENSURE WE'RE ONLY ALLOWED TO ALTER
    //       THE APPROPRIATE RESOURCES
    internal class BlobStorageResourceStore : IResourceStore
    {
        private const string DATA_FILENAME = ".data";
        private const string PERMISSIONS_FILENAME = ".permissions";
        private const string RESOURCE_TYPE_KEY = "ResourceType";

        private readonly BlobContainerClient _Client;

        public BlobStorageResourceStore(BlobContainerClient client)
        {
            _Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task DeleteAsync(params ScopePath[] resourceIds)
        {
            foreach (var rid in resourceIds)
            {
                var dataPath = GetDataPath(rid);
                var permissionsPath = GetPermissionsPath(rid);

                await _Client.GetBlobClient(dataPath).DeleteIfExistsAsync();
                await _Client.GetBlobClient(permissionsPath).DeleteIfExistsAsync();

                // Get rid of all sub-resources
                var prefix = rid.ToString() + "/";
                await foreach (var blob in _Client.GetBlobsAsync(prefix: prefix))
                {
                    await _Client.GetBlobClient(blob.Name).DeleteIfExistsAsync();
                }
            }
        }

        public async Task<T?> GetAsync<T>(ScopePath resourceId) where T : SparcpointResource
        {
            T? data = await GetData<T>(resourceId);
            if (data == null)
                return null;

            var permissions = await GetPermissions(resourceId);

            ResourceIdAttribute.SetResourceId(data, resourceId);
            ResourcePermissionsAttribute.SetPermissions(data, permissions);

            return data;
        }

        public async Task<IEnumerable<SparcpointResourceEntry>> GetChildEntriesAsync(ScopePath parentResourceId, int maxDepth = 1, string[]? includeTypes = null)
        {
            var prefix = parentResourceId.ToString() + "/";
            var checkTypes = (includeTypes != null && includeTypes.Any());

            List<SparcpointResourceEntry> entries = new();
            await foreach(var blob in _Client.GetBlobsAsync(traits: BlobTraits.Tags, prefix: prefix))
            {
                if (!blob.Name.EndsWith(DATA_FILENAME))
                    continue;

                ScopePath resourceId = ScopePath.Parse(blob.Name).Back(1);

                if (resourceId.Rank > (parentResourceId.Rank + maxDepth))
                    continue;

                var tags = blob.Tags;
                if (!tags.ContainsKey(RESOURCE_TYPE_KEY))
                    continue;

                string resourceType = tags[RESOURCE_TYPE_KEY];

                if (checkTypes && !includeTypes!.Contains(resourceType))
                    continue;

                var permissions = await GetPermissions(resourceId);
                entries.Add(new SparcpointResourceEntry(resourceId, resourceType, permissions));
            }

            return entries;
        }

        public async Task SetAsync<T>(T data) where T : SparcpointResource
        {
            string resourceType = ResourceTypeAttribute.GetResourceType(data.GetType());
            var tags = new Dictionary<string, string>
            {
                [RESOURCE_TYPE_KEY] = resourceType,
            };

            var resourceId = data.ResourceId;
            Ensure.NotNullOrWhiteSpace(resourceId);
            Ensure.NotEqual(ScopePath.RootScope, resourceId);

            var dataPath = GetDataPath(resourceId);
            var client = _Client.GetBlobClient(dataPath);
            await client.UpdateAsJsonAsync(data, tags: tags);

            var permissionsPath = GetPermissionsPath(resourceId);
            client = _Client.GetBlobClient(permissionsPath);
            await client.UpdateAsJsonAsync(data.Permissions ?? new());
        }

        private ScopePath GetDataPath(ScopePath resourceId)
            => resourceId + DATA_FILENAME;
        private async Task<T?> GetData<T>(ScopePath resourceId) where T : class
            => await _Client.GetBlobClient(GetDataPath(resourceId)).GetAsJsonAsync<T>();
        private ScopePath GetPermissionsPath(ScopePath resourceId)
            => resourceId + PERMISSIONS_FILENAME;
        private async Task<ResourcePermissions> GetPermissions(ScopePath resourceId)
            => (await _Client.GetBlobClient(GetPermissionsPath(resourceId)).GetAsJsonAsync<ResourcePermissions>()) ?? new();
    }
}
