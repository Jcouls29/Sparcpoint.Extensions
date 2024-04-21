namespace Sparcpoint.Extensions.Multitenancy;

public class AccountTenantResolver : ITenantResolver<AccountTenant>
{
    public TenantContext<AccountTenant> Resolve(HttpContext context)
    {
        Ensure.NotNull(context);

        if (context.Request.RouteValues.TryGetValue("accountId", out object? value) && value != null && value is string accountId)
        {
            return TenantContext<AccountTenant>.TenantFound(new AccountTenant { AccountId = accountId });
        }

        return TenantContext<AccountTenant>.Empty;
    }
}
