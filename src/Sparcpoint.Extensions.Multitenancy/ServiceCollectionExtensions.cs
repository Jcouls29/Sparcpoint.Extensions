using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Sparcpoint.Extensions.Multitenancy;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMultitenancy<TTenant, TResolver>(this IServiceCollection services, Func<TenantServiceBuilder<TTenant>, TenantServiceBuilder<TTenant>> configure)
        where TResolver : class, ITenantResolver<TTenant>
        where TTenant : class
    {
        Ensure.ArgumentNotNull(services);
        Ensure.ArgumentNotNull(configure);

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddScoped<ITenantResolver<TTenant>, TResolver>();
        services.AddScoped<TenantContext<TTenant>>(provider =>
        {
            var resolver = provider.GetRequiredService<ITenantResolver<TTenant>>();
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();
            Ensure.NotNull(accessor.HttpContext);

            return resolver.Resolve(accessor.HttpContext) ?? TenantContext<TTenant>.Empty;
        });

        var builder = new TenantServiceBuilder<TTenant>(services);
        configure(builder);

        return services;
    }
}
