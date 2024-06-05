using Sparcpoint;
using Sparcpoint.Extensions.Permissions;
using Sparcpoint.Extensions.Resources;
using Sparcpoint.Extensions.Resources.InMemory;

namespace Sparcpoint.Extensions.Tests.Resources
{
    public class OrganizationClient_Tests
    {
        public OrganizationClient_Tests()
        {
            Factory = new SparcpointClientFactory(new InMemoryResourceStore(new InMemoryResourceCollection()));
            Client = Factory.Create("ACCOUNT_01");
            Subscription = Client.CreateNewSubscriptionAsync(new SubscriptionData { DisplayName = "SUB 1" }).Result;
            Organization = Subscription.CreateNewOrganizationAsync(new OrganizationData { DisplayName = "ORG 1" }).Result;
        }

        public SparcpointClientFactory Factory { get; }
        public ISparcpointClient Client { get; }
        public ISubscriptionClient Subscription { get; }
        public IOrganizationClient Organization { get; }

        [Fact]
        public async Task Can_delete_confirmed()
        {
            await Organization.DeleteAsync();

            var allOrgs = await Subscription.GetOrganizationsAsync().ToArrayAsync();
            Assert.Empty(allOrgs);
        }

        [Fact]
        public async Task Can_get_permissions()
        {
            var permissions = await Organization.GetPermissionsAsync();
            Assert.NotNull(permissions);
            Assert.Equal(4, permissions.Count);

            AssertIsAllowed(permissions, "ACCOUNT_01", CommonPermissions.CanReadData);
            AssertIsAllowed(permissions, "ACCOUNT_01", CommonPermissions.CanReadPermissions);
            AssertIsAllowed(permissions, "ACCOUNT_01", CommonPermissions.CanWriteData);
            AssertIsAllowed(permissions, "ACCOUNT_01", CommonPermissions.CanWritePermissions);
        }

        [Fact]
        public async Task Can_set_permissions()
        {
            var permissions = new ResourcePermissions
            {
                new ResourcePermissionEntry { AccountId = "ACCOUNT_02", Permission = new PermissionEntry("ToLive", PermissionValue.Allow) }
            };
            await Organization.SetPermissionsAsync(permissions);

            var actualPermissions = await Organization.GetPermissionsAsync();
            Assert.NotNull(actualPermissions);
            Assert.Single(actualPermissions);
            AssertIsAllowed(actualPermissions, "ACCOUNT_02", "ToLive");
        }

        [Fact]
        public async Task Can_create_subresource()
        {
            var client = await CreateProject("ProjectA", "Default");
            Assert.NotNull(client);
            
            var value = await client.GetAsync();
            Assert.NotNull(value);
            Assert.Equal("ProjectA", value.ProjectName);
            Assert.Equal("Default", value.ProjectType);
            Assert.Equal(client.ResourceId, value.ResourceId);
            Assert.True(client.ResourceId.StartsWith(Organization.ResourceId));
        }

        [Fact]
        public async Task Can_create_sub_sub_resource_directly()
        {
            var projectClient = await CreateProject("ProjectA", "Default");
            var widgetClient = await CreateWidget("ProjectA", "TextWidget");

            var project = await projectClient.GetAsync();
            var widget = await widgetClient.GetAsync();

            Assert.NotNull(project);
            Assert.NotNull(widget);

            Assert.True(widget.ResourceId.StartsWith(Organization.ResourceId));
            Assert.True(widget.ResourceId.StartsWith(project.ResourceId));
        }

        [Fact]
        public async Task Can_find_sub_sub_resources()
        {
            await CreateWidget("ProjectA", "TextWidget");
            await CreateWidget("ProjectA", "LineWidget");
            await CreateWidget("ProjectB", "ColorWidget");

            var widgets = await Task.WhenAll(await Organization.GetChildClientsAsync<ProjectWidgetData>(maxDepth: 5).Select(c => c.GetAsync()).ToArrayAsync());

            Assert.Equal(3, widgets.Count());
            Assert.Contains(widgets, w => w!.WidgetType == "TextWidget");
            Assert.Contains(widgets, w => w!.WidgetType == "LineWidget");
            Assert.Contains(widgets, w => w!.WidgetType == "ColorWidget");
        }

        [Fact]
        public async Task Can_list_sub_sub_resources()
        {
            await CreateWidget("ProjectA", "TextWidget");
            await CreateWidget("ProjectA", "LineWidget");
            await CreateWidget("ProjectB", "ColorWidget");

            var list = await Organization.ListChildrenAsync<ProjectWidgetData>((w) => w.WidgetType, maxDepth: 5).ToArrayAsync();
            Assert.Equal(3, list.Length);

            Assert.Contains(list, l => l.DisplayValue == "TextWidget");
            Assert.Contains(list, l => l.DisplayValue == "LineWidget");
            Assert.Contains(list, l => l.DisplayValue == "ColorWidget");
        }

        [Fact]
        public async Task Max_depth_less_than_resources_then_nothing_returns()
        {
            await CreateWidget("ProjectA", "TextWidget");
            await CreateWidget("ProjectA", "LineWidget");
            await CreateWidget("ProjectB", "ColorWidget");

            var widgets = await Organization.GetChildClientsAsync<ProjectWidgetData>(maxDepth: 4).ToArrayAsync();
            Assert.Empty(widgets);

            var list = await Organization.ListChildrenAsync<ProjectWidgetData>(w => w.WidgetType, maxDepth: 4).ToArrayAsync();
            Assert.Empty(list);
        }

        [Fact]
        public async Task Can_find_by_child_prefix()
        {
            var project = await CreateProject("ProjectA", "Default");
            await CreateWidget("ProjectA", "TextWidget");
            await CreateWidget("ProjectA", "LineWidget");
            await CreateWidget("ProjectB", "ColorWidget");

            var found = await Task.WhenAll(await Organization.GetChildClientsAsync<ProjectWidgetData>(project.ResourceId - Organization.ResourceId)
                .Select(w => w.GetAsync())
                .ToArrayAsync());

            Assert.Equal(2, found.Length);
            Assert.Contains(found, w => w!.WidgetType == "TextWidget");
            Assert.Contains(found, w => w!.WidgetType == "LineWidget");
        }

        [Fact]
        public async Task Can_find_widgets_from_subscription_level()
        {
            await CreateWidget("ProjectA", "TextWidget");
            await CreateWidget("ProjectA", "LineWidget");
            await CreateWidget("ProjectB", "ColorWidget");

            var found = await Subscription.GetChildrenAsync<ProjectWidgetData>().ToArrayAsync();
            Assert.Equal(3, found.Length);
            Assert.Contains(found, w => w.WidgetType == "TextWidget");
            Assert.Contains(found, w => w.WidgetType == "LineWidget");
            Assert.Contains(found, w => w.WidgetType == "ColorWidget");
        }

        private void AssertIsAllowed(ResourcePermissions permissions, string accountId, string key)
            => Assert.Contains(permissions, (p) => p.AccountId == accountId && p.Permission.Key == key && (p.Permission.IsAllowed ?? false));

        private async Task<IResourceDataClient<OrganizationProjectData>> CreateProject(string projectName, string projectType)
        {
            var relativeResourceId = "/projects/" + projectName.Slugify();
            return await Organization.SaveAsync(relativeResourceId, new OrganizationProjectData { ProjectName = projectName, ProjectType = projectType });
        }

        private async Task<IResourceDataClient<ProjectWidgetData>> CreateWidget(string projectName, string widgetType, Dictionary<string, string>? data = null)
        {
            var relativeResourceId = "/projects/" + projectName.Slugify() + "/widgets/" + widgetType.Slugify() + "/" + Guid.NewGuid().ToString();
            return await Organization.SaveAsync(relativeResourceId, new ProjectWidgetData { WidgetData = data ?? new(), WidgetType = widgetType });
        }
    }
}

[ResourceType("Organization.Project")]
public class OrganizationProjectData
{
    [ResourceId]
    public ScopePath ResourceId { get; set; }

    public string ProjectName { get; set; } = string.Empty;
    public string ProjectType { get; set; } = string.Empty;
}

[ResourceType("Organization.Project.Widget")]
public class ProjectWidgetData
{
    [ResourceId]
    public ScopePath ResourceId { get; set; }

    public string WidgetType { get; set; } = string.Empty;
    public Dictionary<string, string> WidgetData { get; set; } = new();
} 