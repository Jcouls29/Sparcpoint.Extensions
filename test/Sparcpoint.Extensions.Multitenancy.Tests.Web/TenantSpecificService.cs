namespace Sparcpoint.Extensions.Multitenancy;

public class TenantSpecificService
{
    public TenantSpecificService(AccountTenant tenant)
    {
        Tenant = tenant;
    }

    public AccountTenant? Tenant { get; }
}