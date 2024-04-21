using Microsoft.AspNetCore.Http;

namespace Sparcpoint.Extensions.Multitenancy;

public interface ITenantResolver<TTenant>
    where TTenant : class
{
    TenantContext<TTenant> Resolve(HttpContext context);
}