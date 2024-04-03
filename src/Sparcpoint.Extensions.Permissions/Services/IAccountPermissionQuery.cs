namespace Sparcpoint.Extensions.Permissions;

public interface IAccountPermissionQuery
{
    IAsyncEnumerable<AccountPermissionEntry> RunAsync(PermissionQueryParameters parameters);
}

public class PermissionQueryParameters
{
    public string? AccountId { get; set; } = null;

    public string? KeyExact { get; set; } = null;
    public string? KeyStartsWith { get; set; } = null;
    public string? KeyEndsWith { get; set; } = null;
    public ScopePath? ScopeExact { get;set; } = null;
    public ScopePath? ScopeStartsWith { get; set; } = null;
    public ScopePath? ScopeEndsWith { get; set; } = null;
    public PermissionValue? ValueExact { get; set; } = null;
    public bool ImmediateChildrenOnly { get; set; } = false;
    public Dictionary<string, string>? WithMetadata { get; set; } = null;
}