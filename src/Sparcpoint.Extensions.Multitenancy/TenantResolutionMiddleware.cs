using Microsoft.AspNetCore.Http;

namespace Sparcpoint.Extensions.Multitenancy;

internal sealed class TenantResolutionMiddleware<TTenant>
    where TTenant : class
{
    private readonly RequestDelegate _Next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        Ensure.ArgumentNotNull(next);

        _Next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver<TTenant> tenantResolver)
    {
        Ensure.ArgumentNotNull(context);
        Ensure.ArgumentNotNull(tenantResolver);

        var tenantContext = await tenantResolver.ResolveAsync(context);

        if (tenantContext != null)
        {
            context.SetTenantContext(tenantContext);
        }

        await _Next.Invoke(context);
    }
}
