using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Tests.Permissions;

public class ScopePath_Tests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("//")]
    [InlineData("///")]
    [InlineData("\\")]
    [InlineData("\\\\")]
    [InlineData("\\\\///\\\\")]
    public void Equals_root_scope(string? path)
    {
        Assert.Equal(ScopePath.RootScope, ScopePath.Parse(path));
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("/", 0)]
    [InlineData("//", 0)]
    [InlineData("///", 0)]
    [InlineData("/Original", 1)]
    [InlineData("\\Original", 1)]
    [InlineData("/Original/", 1)]
    [InlineData("/Original///", 1)]
    [InlineData("/Original/Child", 2)]
    [InlineData("/Original/Child/", 2)]
    [InlineData("/Original/Child//////", 2)]
    [InlineData("/Original/Child/SubChild", 3)]
    [InlineData("/Original/Child/SubChild/", 3)]
    [InlineData("/Original/Child/SubChild////", 3)]
    [InlineData("/Original\\Child\\SubChild//\\", 3)]
    public void Rank_properly_calculated(string? path, int rank)
    {
        var scope = ScopePath.Parse(path);
        Assert.Equal(rank, scope.Rank);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "\\\\")]
    [InlineData("\\", "/")]
    [InlineData("", "")]
    [InlineData("/ORIGINAL", "ORIGINAL")]
    [InlineData("//ORIGINAL//", "\\ORIGINAL\\")]
    [InlineData("/O/S/2/1/P/S", "\\O\\S/2\\1/P/S/")]
    public void Scopes_are_equal(string? leftPath, string? rightPath)
    {
        var leftScope = ScopePath.Parse(leftPath);
        var rightScope = ScopePath.Parse(rightPath);

        Assert.Equal(leftScope, rightScope);
    }

    [Theory]
    [InlineData("", "/original")]
    [InlineData("", "/original/sublevel")]
    [InlineData("", "/original/sublevel/2")]
    [InlineData("/original", "/original/sublevel")]
    [InlineData("/original/sublevel/", "/original/sublevel/2")]
    public void Scopes_operator_less_than(string? leftPath, string? rightPath)
    {
        var leftScope = ScopePath.Parse(leftPath);
        var rightScope = ScopePath.Parse(rightPath);

        Assert.True(leftScope < rightScope);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("", "/original")]
    [InlineData("/original/", "/original")]
    [InlineData("", "/original/sublevel")]
    [InlineData("", "/original/sublevel/2")]
    [InlineData("/original", "/original/sublevel")]
    [InlineData("/original/sublevel/", "/original/sublevel/2")]
    [InlineData("/original/sublevel/2", "/original/sublevel/2/")]
    public void Scopes_operator_less_than_equals(string? leftPath, string? rightPath)
    {
        var leftScope = ScopePath.Parse(leftPath);
        var rightScope = ScopePath.Parse(rightPath);

        Assert.True(leftScope <= rightScope);
    }

    [Theory]
    [InlineData("/original", "")]
    [InlineData("/original/sublevel", "")]
    [InlineData("original/sublevel/2", "")]
    [InlineData("original/sublevel", "original")]
    [InlineData("original/sublevel/2", "/original")]
    [InlineData("/o/s/2/3", "/o/s/2")]
    public void Scopes_operator_greater_than(string? leftPath, string? rightPath)
    {
        var leftScope = ScopePath.Parse(leftPath);
        var rightScope = ScopePath.Parse(rightPath);

        Assert.True(leftScope > rightScope);
    }

    [Theory]
    [InlineData("/", "")]
    [InlineData("/original", "original")]
    [InlineData("/original", "")]
    [InlineData("/original/sublevel", "")]
    [InlineData("original/sublevel/2", "")]
    [InlineData("original/sublevel/2", "/original/sublevel/2/")]
    [InlineData("original/sublevel", "original")]
    [InlineData("original/sublevel/2", "/original")]
    [InlineData("/o/s/2/3", "/o/s/2")]
    [InlineData("/o/s/2/3", "/o/s/2/3/")]
    public void Scopes_operator_greater_than_equals(string? leftPath, string? rightPath)
    {
        var leftScope = ScopePath.Parse(leftPath);
        var rightScope = ScopePath.Parse(rightPath);

        Assert.True(leftScope >= rightScope);
    }

    [Theory]
    [InlineData("", "", "/")]
    [InlineData("/", "/", "/")]
    [InlineData("original", "", "/original")]
    [InlineData("original", "sublevel", "/original/sublevel")]
    [InlineData("/", "original", "/original")]
    [InlineData("/original", "", "/original")]
    [InlineData("/original/sublevel", "/level3/level4/level5", "/original/sublevel/level3/level4/level5")]
    public void Scopes_append_properly(string leftPath, string rightPath, string finalPath)
    {
        var leftScope = ScopePath.Parse(leftPath);
        var rightScope = ScopePath.Parse(rightPath);
        var finalScope = ScopePath.Parse(finalPath);

        Assert.Equal(finalScope, leftScope & rightScope);
    }

    [Theory]
    [InlineData("/original", "/new")]
    [InlineData("/original/second", "/original/third")]
    [InlineData("/original/second", "/new/second")]
    public void No_scope_overlap(string left, string right)
    {
        ScopePath leftScope = ScopePath.Parse(left);
        ScopePath rightScope = ScopePath.Parse(right);

        Assert.False(leftScope < rightScope);
        Assert.False(leftScope > rightScope);
        Assert.False(leftScope <= rightScope);
        Assert.False(leftScope >= rightScope);
        Assert.False(leftScope == rightScope);
        Assert.True(leftScope !=  rightScope);
    }

    [Theory]
    [InlineData("/original", "/other", "/")]
    [InlineData("/original", "/original/secondary", "/original")]
    [InlineData("/original/secondary/third", "/original", "/original")]
    [InlineData("/original/secondary/third", "/original/secondary/fourth", "/original/secondary")]
    public void Branch_points_valid(string left, string right, string branch)
    {
        var leftScope = ScopePath.Parse(left);
        var rightScope = ScopePath.Parse(right);
        var branchScope = ScopePath.Parse(branch);

        Assert.Equal(branch, leftScope.GetBranchPoint(rightScope));
    }

    [Fact]
    public void GetHierarchy_works_without_rootscope()
    {
        ScopePath path = "/Segment1/Cool2/Blue3/Red4/";
        var hierarchy = path.GetHierarchy(includeRootScope: false);

        Assert.Equal(4, hierarchy.Length);
        Assert.Equal("/Segment1/Cool2/Blue3/Red4", hierarchy[0]);
        Assert.Equal("/Segment1/Cool2/Blue3", hierarchy[1]);
        Assert.Equal("/Segment1/Cool2", hierarchy[2]);
        Assert.Equal("/Segment1", hierarchy[3]);
    }

    [Fact]
    public void GetHierarchy_works_with_rootscope()
    {
        ScopePath path = "/Segment1/Cool2/Blue3/Red4/";
        var hierarchy = path.GetHierarchy(includeRootScope: true);

        Assert.Equal(5, hierarchy.Length);
        Assert.Equal("/Segment1/Cool2/Blue3/Red4", hierarchy[0]);
        Assert.Equal("/Segment1/Cool2/Blue3", hierarchy[1]);
        Assert.Equal("/Segment1/Cool2", hierarchy[2]);
        Assert.Equal("/Segment1", hierarchy[3]);
        Assert.Equal("/", hierarchy[4]);
    }

    [Fact]
    public void GetHierarchyAscending_works_without_rootscope()
    {
        ScopePath path = "/Segment1/Cool2/Blue3/Red4/";
        var hierarchy = path.GetHierarchyAscending(includeRootScope: false);

        Assert.Equal(4, hierarchy.Length);
        Assert.Equal("/Segment1/Cool2/Blue3/Red4", hierarchy[3]);
        Assert.Equal("/Segment1/Cool2/Blue3", hierarchy[2]);
        Assert.Equal("/Segment1/Cool2", hierarchy[1]);
        Assert.Equal("/Segment1", hierarchy[0]);
    }

    [Fact]
    public void GetHierarchyAscending_works_with_rootscope()
    {
        ScopePath path = "/Segment1/Cool2/Blue3/Red4/";
        var hierarchy = path.GetHierarchyAscending(includeRootScope: true);

        Assert.Equal(5, hierarchy.Length);
        Assert.Equal("/Segment1/Cool2/Blue3/Red4", hierarchy[4]);
        Assert.Equal("/Segment1/Cool2/Blue3", hierarchy[3]);
        Assert.Equal("/Segment1/Cool2", hierarchy[2]);
        Assert.Equal("/Segment1", hierarchy[1]);
        Assert.Equal("/", hierarchy[0]);
    }
}
