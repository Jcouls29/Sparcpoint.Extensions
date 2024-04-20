using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Sparcpoint.Extensions.Multitenancy;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMultitenancy<TTenant, TResolver>(this IServiceCollection services, Func<TenantServiceBuilder<TTenant>, TenantServiceBuilder<TTenant>> configure)
        where TResolver : class, ITenantResolver<TTenant>
        where TTenant : class
    {
        Ensure.ArgumentNotNull(services);
        Ensure.ArgumentNotNull(configure);

        services.AddScoped<ITenantResolver<TTenant>, TResolver>();
        services.AddScoped<TenantContext<TTenant>>(provider =>
        {
            var resolver = provider.GetRequiredService<ITenantResolver<TTenant>>();
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();
            Ensure.NotNull(accessor.HttpContext);

            return resolver.Resolve(accessor.HttpContext);
        });
        services.AddScoped<TTenant>(provider => provider.GetRequiredService<TenantContext<TTenant>>().Tenant);

        var builder = new TenantServiceBuilder<TTenant>(services);
        configure(builder);

        return services;
    }
}
