using Microsoft.AspNetCore.Http;

namespace Sparcpoint.Extensions.Multitenancy;

public interface ITenantResolver<TTenant>
{
    TenantContext<TTenant> Resolve(HttpContext context);
}