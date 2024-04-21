using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Sparcpoint.Extensions.Multitenancy;

public sealed class TenantServiceBuilder<TTenant>
    where TTenant : class
{
    private readonly IServiceCollection _Services;

    internal TenantServiceBuilder(IServiceCollection services)
    {
        Ensure.ArgumentNotNull(services);

        _Services = services;
    }

    public TenantServiceBuilder<TTenant> Add<TService>() 
        where TService : class
    {
        _Services.AddScoped<TService>();

        return this;
    }

    public TenantServiceBuilder<TTenant> Add<TService, TImplementation>() 
        where TImplementation : class, TService 
        where TService : class
    {
        _Services.AddScoped<TService, TImplementation>();

        return this;
    }

    public TenantServiceBuilder<TTenant> Add<TService>(Func<TTenant, IServiceProvider, TService> configure) 
        where TService : class
    {
        _Services.AddScoped(provider => configure(provider.GetRequiredService<TTenant>(), provider));

        return this;
    }

    public TenantServiceBuilder<TTenant> Add<TService, TImplementation>(Func<TTenant, IServiceProvider, TImplementation> configure) 
        where TImplementation : class, TService 
        where TService : class
    {
        _Services.AddScoped<TService, TImplementation>(provider => configure(provider.GetRequiredService<TTenant>(), provider));

        return this;
    }
}