using Microsoft.AspNetCore.Http;

namespace Sparcpoint.Extensions.Multitenancy;

public interface ITenantResolver<TTenant>
    where TTenant : class
{
    Task<TenantContext<TTenant>?> ResolveAsync(HttpContext context);
}
