using Microsoft.AspNetCore.Http;

namespace Sparcpoint.Extensions.Multitenancy;

public static class HttpContextExtensions
{
    private const string TENANT_CONTEXT_KEY = "sparcpoint:tenant-context";

    public static void SetTenantContext<TTenant>(this HttpContext context, TenantContext<TTenant> tenantContext)
    {
        Ensure.ArgumentNotNull(context);
        Ensure.ArgumentNotNull(tenantContext);

        context.Items[TENANT_CONTEXT_KEY] = tenantContext;
    }

    public static TenantContext<TTenant>? GetTenantContext<TTenant>(this HttpContext context)
    {
        Ensure.ArgumentNotNull(context);

        if (context.Items.TryGetValue(TENANT_CONTEXT_KEY, out object? value) 
            && value != null 
            && value is TenantContext<TTenant> tenantContext)
        {
            return tenantContext;
        }

        return null;
    }

    public static TTenant? GetTenant<TTenant>(this HttpContext context)
    {
        var tenantContext = GetTenantContext<TTenant>(context);
        if (tenantContext == null)
            return default;

        return tenantContext.Tenant;
    }
}
