namespace Sparcpoint.Extensions.Permissions;

public interface IAccountPermissionQuery
{
    Task<IEnumerable<PermissionEntry>> RunAsync(string accountId, PermissionQueryParameters parameters);
    Task<bool> HasAccessAsync(string accountId, string key, ScopePath? scope = null);
}

public class PermissionQueryParameters
{
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