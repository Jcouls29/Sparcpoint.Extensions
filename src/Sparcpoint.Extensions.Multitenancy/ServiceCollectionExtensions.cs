using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Sparcpoint.Extensions.Multitenancy;
#pragma warning disable CS8603 // Possible null reference return.

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMultitenancy<TTenant, TResolver>(this IServiceCollection services, Func<TenantServiceBuilder<TTenant>, TenantServiceBuilder<TTenant>> configure)
        where TResolver : class, ITenantResolver<TTenant>
        where TTenant : class
    {
        Ensure.ArgumentNotNull(services);
        Ensure.ArgumentNotNull(configure);

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddSingleton(typeof(ITenantAccessor<>), typeof(DefaultTenantAccessor<>));
        
        services.AddScoped<ITenantResolver<TTenant>, TResolver>();
        services.AddScoped<TenantContext<TTenant>>((p) =>
        {
            var accessor = p.GetService<ITenantAccessor<TTenant>>();
            return accessor?.Context;
        });
        services.AddScoped<TTenant>((p) =>
        {
            var accessor = p.GetService<ITenantAccessor<TTenant>>();
            return accessor?.Context?.Tenant;
        });

        var builder = new TenantServiceBuilder<TTenant>(services);
        configure(builder);

        return services;
    }
}
