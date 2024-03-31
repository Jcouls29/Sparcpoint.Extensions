using Sparcpoint.Extensions.Objects;

namespace Sparcpoint.Extensions.IntegrationTests;

public class BlobStorageObjectStore_Query : IClassFixture<BlobStorageFixture>
{
    private readonly BlobStorageFixture _Fixture;

    public BlobStorageObjectStore_Query(BlobStorageFixture fixture)
    {
        _Fixture = fixture;
    }

    [Theory]
    [InlineData("/organizations/org_01", "Organization 01", "My Organization")]
    [InlineData("/organizations/org_01/projects/prj_01", "Project 01", "My Project 01")]
    [InlineData("/organizations/org_02", "Organization 02", "My Organization 2")]
    [InlineData("/rules/rule_01", "Rule 01", null)]
    public async Task Id_only(string id, string name, string? searchableString)
    {
        var found = await _Fixture.ObjectQuery.FirstOrDefaultAsync(new ObjectQueryParameters { Id = id });
        Assert.NotNull(found);
        Assert.Equal(id, found.Id);
        Assert.Equal(name, found.Name);
        Assert.Equal(searchableString, found.SearchableString);
    }

    [Theory]
    [InlineData("/organizations/org_01", "Organization 01", "My Organization")]
    [InlineData("/organizations/org_01/projects/prj_01", "Project 01", "My Project 01")]
    [InlineData("/organizations/org_02", "Organization 02", "My Organization 2")]
    [InlineData("/rules/rule_01", "Rule 01", null)]
    public async Task Id_and_name(string id, string name, string? searchableString)
    {
        var found = await _Fixture.ObjectQuery.FirstOrDefaultAsync(new ObjectQueryParameters { Id = id, Name = name });
        Assert.NotNull(found);
        Assert.Equal(id, found.Id);
        Assert.Equal(name, found.Name);
        Assert.Equal(searchableString, found.SearchableString);
    }

    [Theory]
    [InlineData("/organizations/org_01", "Organization 01", "My Organization")]
    [InlineData("/organizations/org_01/projects/prj_01", "Project 01", "My Project 01")]
    [InlineData("/organizations/org_02", "Organization 02", "My Organization 2")]
    [InlineData("/rules/rule_01", "Rule 01", null)]
    public async Task Id_and_name_and_search(string id, string name, string? searchableString)
    {
        var found = await _Fixture.ObjectQuery.FirstOrDefaultAsync(new ObjectQueryParameters { Id = id, Name = name, WithProperties = new Dictionary<string, string>
        {
            ["SearchableString"] = searchableString,
        }});
        Assert.NotNull(found);
        Assert.Equal(id, found.Id);
        Assert.Equal(name, found.Name);
        Assert.Equal(searchableString, found.SearchableString);
    }

    [Theory]
    [InlineData("/organizations/org_01", "Organization 01", "My Organization")]
    [InlineData("/organizations/org_01/projects/prj_01", "Project 01", "My Project 01")]
    [InlineData("/organizations/org_02", "Organization 02", "My Organization 2")]
    [InlineData("/rules/rule_01", "Rule 01", null)]
    public async Task Name_only(string id, string name, string? searchableString)
    {
        var found = await _Fixture.ObjectQuery.FirstOrDefaultAsync(new ObjectQueryParameters
        {
            Name = name
        });
        Assert.NotNull(found);
        Assert.Equal(id, found.Id);
        Assert.Equal(name, found.Name);
        Assert.Equal(searchableString, found.SearchableString);
    }

    [Theory]
    [InlineData("/organizations/org_01", "Organization 01", "My Organization")]
    [InlineData("/organizations/org_01/projects/prj_01", "Project 01", "My Project 01")]
    [InlineData("/organizations/org_02", "Organization 02", "My Organization 2")]
    [InlineData("/rules/rule_01", "Rule 01", null)]
    public async Task Name_and_search(string id, string name, string? searchableString)
    {
        var found = await _Fixture.ObjectQuery.FirstOrDefaultAsync(new ObjectQueryParameters
        {
            Name = name,
            WithProperties = new Dictionary<string, string>
            {
                ["SearchableString"] = searchableString,
            }
        });
        Assert.NotNull(found);
        Assert.Equal(id, found.Id);
        Assert.Equal(name, found.Name);
        Assert.Equal(searchableString, found.SearchableString);
    }

}
