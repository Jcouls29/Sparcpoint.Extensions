namespace Sparcpoint.Extensions.Multitenancy;

public interface ITenantAccessor<TTenant>
    where TTenant : class
{
    TenantContext<TTenant>? Context { get; }
}