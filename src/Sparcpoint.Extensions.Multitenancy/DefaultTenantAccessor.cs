using Microsoft.AspNetCore.Http;

namespace Sparcpoint.Extensions.Multitenancy;

internal class DefaultTenantAccessor<TTenant> : ITenantAccessor<TTenant>
    where TTenant : class
{
    private readonly IHttpContextAccessor _Accessor;

    public DefaultTenantAccessor(IHttpContextAccessor accessor)
    {
        Ensure.ArgumentNotNull(accessor);

        _Accessor = accessor;
    }

    public TenantContext<TTenant>? Context => _Accessor.HttpContext?.GetTenantContext<TTenant>();
}
