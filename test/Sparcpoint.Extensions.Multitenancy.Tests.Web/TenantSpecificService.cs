namespace Sparcpoint.Extensions.Multitenancy;

public class TenantSpecificService
{
    public TenantSpecificService(TenantContext<AccountTenant> tenant)
    {
        Tenant = tenant.Tenant;
    }

    public AccountTenant? Tenant { get; }
}