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
    [InlineData("/ORIGINAL", "original")]
    [InlineData("//ORIGINAL//", "\\original\\")]
    [InlineData("/O/S/2/1/P/S", "\\o\\s/2\\1/p/s/")]
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
}
