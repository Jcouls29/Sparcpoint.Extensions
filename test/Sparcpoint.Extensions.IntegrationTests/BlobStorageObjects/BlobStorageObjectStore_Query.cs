using Sparcpoint.Extensions.Objects;
using Xunit.Abstractions;

namespace Sparcpoint.Extensions.IntegrationTests;

public class BlobStorageObjectStore_Query : IClassFixture<BlobStorageFixture>
{
    private readonly BlobStorageFixture _Fixture;
    private readonly ITestOutputHelper _Output;

    public BlobStorageObjectStore_Query(BlobStorageFixture fixture, ITestOutputHelper output)
    {
        _Fixture = fixture;
        _Output = output;
    }

    [Theory]
    [InlineData("/organizations/org_01", "Organization 01", "My Organization")]
    [InlineData("/organizations/org_01/projects/prj_01", "Project 01", "My Project")]
    [InlineData("/organizations/org_02", "Organization 02", "My Organization")]
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
    [InlineData("/organizations/org_01/projects/prj_01", "Project 01", "My Project")]
    [InlineData("/organizations/org_02", "Organization 02", "My Organization")]
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
    [InlineData("/organizations/org_01/projects/prj_01", "Project 01", "My Project")]
    [InlineData("/organizations/org_02", "Organization 02", "My Organization")]
    [InlineData("/rules/rule_01", "Rule 01", null)]
    public async Task Id_and_name_and_search(string id, string name, string? searchableString)
    {
        var found = await _Fixture.ObjectQuery.FirstOrDefaultAsync(new ObjectQueryParameters 
        { 
            Id = id, 
            Name = name, 
            WithProperties = new Dictionary<string, string?>
            {
                ["SearchableString"] = searchableString,
            }
        });
        Assert.NotNull(found);
        Assert.Equal(id, found.Id);
        Assert.Equal(name, found.Name);
        Assert.Equal(searchableString, found.SearchableString);
    }

    [Theory]
    [InlineData("/organizations/org_01", "Organization 01", "My Organization")]
    [InlineData("/organizations/org_01/projects/prj_01", "Project 01", "My Project")]
    [InlineData("/organizations/org_02", "Organization 02", "My Organization")]
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
    [InlineData("/organizations/org_01/projects/prj_01", "Project 01", "My Project")]
    [InlineData("/organizations/org_02", "Organization 02", "My Organization")]
    [InlineData("/rules/rule_01", "Rule 01", null)]
    public async Task Name_and_search(string id, string name, string? searchableString)
    {
        var found = await _Fixture.ObjectQuery.FirstOrDefaultAsync(new ObjectQueryParameters
        {
            Name = name,
            WithProperties = new Dictionary<string, string?>
            {
                ["SearchableString"] = searchableString,
            }
        });

        Assert.NotNull(found);
        Assert.Equal(id, found.Id);
        Assert.Equal(name, found.Name);
        Assert.Equal(searchableString, found.SearchableString);
    }

    [Fact]
    public async Task ParentScope_only()
    {
        var found = await _Fixture.ObjectQuery.ToListAsync(new ObjectQueryParameters { ParentScope = "/organizations/org_01/projects" });

        Assert.NotNull(found);
        Assert.Equal(3, found.Count);

        Assert.Contains(found, (v) => v.Id == "/organizations/org_01/projects/prj_01");
        Assert.Contains(found, (v) => v.Id == "/organizations/org_01/projects/prj_02");
        Assert.Contains(found, (v) => v.Id == "/organizations/org_01/projects/prj_03");
    }

    [Fact]
    public async Task ParentScope_and_name()
    {
        var found = await _Fixture.ObjectQuery.ToListAsync(new ObjectQueryParameters 
        { 
            ParentScope = "/organizations/org_01/projects",
            Name = "Project 02"
        });
        Assert.NotNull(found);
        Assert.Single(found);
        Assert.Contains(found, (v) => v.Id == "/organizations/org_01/projects/prj_02");
    }

    [Fact]
    public async Task ParentScope_and_property()
    {
        var found = await _Fixture.ObjectQuery.ToListAsync(new ObjectQueryParameters
        {
            ParentScope = "/organizations/org_01/projects",
            WithProperties = new Dictionary<string, string?>
            {
                ["SearchableString"] = "My Project"
            }
        });
        Assert.NotNull(found);
        Assert.Equal(3, found.Count);
        Assert.All(found, (v) => Assert.Equal("My Project", v.SearchableString));
    }

    [Theory]
    [InlineData("Project", 5)]
    [InlineData("Organization", 4)]
    [InlineData("Rule", 1)]
    public async Task NameStartsWith_only(string startsWith, int count)
    {
        var found = await _Fixture.ObjectQuery.ToListAsync(new ObjectQueryParameters
        {
            NameStartsWith = startsWith,
        });

        Assert.All(found, (v) => _Output.WriteLine($"Found: {v.Id}"));

        Assert.NotNull(found);
        Assert.Equal(count, found.Count);
        Assert.All(found, (v) => Assert.StartsWith(startsWith, v.Name));
    }

    [Theory]
    [InlineData(" 01", 4)]
    [InlineData(" 02", 2)]
    [InlineData(" 03", 2)]
    [InlineData(" 04", 2)]
    public async Task NameEndWith_only(string endsWith, int count)
    {
        var found = await _Fixture.ObjectQuery.ToListAsync(new ObjectQueryParameters
        {
            NameEndsWith = endsWith,
        });

        Assert.All(found, (v) => _Output.WriteLine($"Found: {v.Id}"));

        Assert.NotNull(found);
        Assert.Equal(count, found.Count);
        Assert.All(found, (v) => Assert.EndsWith(endsWith, v.Name));
    }

    [Theory]
    [InlineData("My Organization", 2)]
    [InlineData("Other Organization", 2)]
    [InlineData("My Project", 4)]
    [InlineData("Other Project", 1)]
    [InlineData(null, 1)]
    public async Task WithPropertiesOnly(string value, int count)
    {
        var found = await _Fixture.ObjectQuery.ToListAsync(new ObjectQueryParameters
        {
            WithProperties = new Dictionary<string, string?>
            {
                ["SearchableString"] = value
            }
        });

        Assert.All(found, (v) => _Output.WriteLine($"Found: {v.Id}"));

        Assert.NotNull(found);
        Assert.Equal(count, found.Count);
        Assert.All(found, (v) => Assert.Equal(value, v.SearchableString));
    }
}
