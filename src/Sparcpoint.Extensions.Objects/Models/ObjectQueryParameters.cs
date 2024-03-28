namespace Sparcpoint.Extensions.Objects;

public record ObjectQueryParameters
{
    public ScopePath? Id { get; set; } = null;
    public ScopePath? ParentScope { get; set; } = null;

    public string? Name { get; set; } = null;
    public string? NameStartsWith { get; set; } = null;
    public string? NameEndsWith { get; set; } = null;

    public Dictionary<string, string>? WithProperties { get; set; } = null;

    internal Type? WithType { get; set; } = null;
}
