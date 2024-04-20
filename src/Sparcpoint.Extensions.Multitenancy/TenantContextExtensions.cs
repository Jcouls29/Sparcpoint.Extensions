namespace Sparcpoint.Extensions.Multitenancy;

internal static class TenantContextExtensions
{
    private const string TENANT_PROVIDER_KEY = "sparcpoint:tenant-container";

    public static IServiceProvider? GetTenantProvider<TTenant>(this TenantContext<TTenant> tenantContext)
    {
        Ensure.ArgumentNotNull(tenantContext);

        if (tenantContext.Properties.TryGetValue(TENANT_PROVIDER_KEY, out object? provider) && provider != null && provider is IServiceProvider serviceProvider)
            return serviceProvider;

        return null;
    }

    public static void SetTenantProvider<TTenant>(this TenantContext<TTenant> tenantContext, IServiceProvider tenantProvider)
    {
        Ensure.ArgumentNotNull(tenantContext);
        Ensure.ArgumentNotNull(tenantProvider);

        tenantContext.Properties[TENANT_PROVIDER_KEY] = tenantProvider;
    }
}
