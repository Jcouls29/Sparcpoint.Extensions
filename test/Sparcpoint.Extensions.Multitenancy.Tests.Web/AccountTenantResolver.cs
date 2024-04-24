namespace Sparcpoint.Extensions.Multitenancy;

#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
public class AccountTenantResolver : ITenantResolver<AccountTenant>
{

    public async Task<TenantContext<AccountTenant>> ResolveAsync(HttpContext context)
    {
        Ensure.NotNull(context);

        if (context.Request.RouteValues.TryGetValue("accountId", out object? value) && value != null && value is string accountId)
        {
            return TenantContext<AccountTenant>.TenantFound(new AccountTenant { AccountId = accountId });
        }

        return TenantContext<AccountTenant>.Empty;
    }
}
