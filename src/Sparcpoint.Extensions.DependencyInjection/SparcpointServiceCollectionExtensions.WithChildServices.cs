using System.Data.Common;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class SparcpointServiceCollectionExtensions
{
    public static IServiceCollection WithChildServices<TService>(this IServiceCollection services, Action<IServiceCollection> configure, ServiceLifetime lifetime = ServiceLifetime.Singleton) where TService : class
        => WithChildServices<TService, TService>(services, configure, lifetime);

    public static IServiceCollection WithChildServices<TService, TImplementation>(this IServiceCollection services, Action<IServiceCollection> configure, ServiceLifetime lifetime = ServiceLifetime.Singleton) where TImplementation : TService
        => WithChildServices(services, typeof(TService), typeof(TImplementation), configure, lifetime);

    public static IServiceCollection WithChildServices(this IServiceCollection services, Type serviceType, Type implementationType, Action<IServiceCollection> configure, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        if (serviceType.IsGenericTypeDefinition)
            throw new InvalidOperationException("Open generics are not supported.");

        bool hasBeenRouted = false;

        // NOTE: This approach allows for many nested coalesced providers.
        //       This should not cause any issues.
        var childServices = new ServiceCollection();
        OwnedProvider childProvider = new OwnedProvider();

        Func<IServiceProvider, object> factory = (IServiceProvider provider) =>
        {
            if (!hasBeenRouted)
            {
                // NOTE: Creates a route to the original registration
                //       Uses transient scope to defer to the parent's
                //       scope instead
                foreach (var service in services.Reverse())
                {
                    if (service.ServiceType.ContainsGenericParameters && service.ImplementationType != null)
                    {
                        childServices.Insert(0, new ServiceDescriptor(service.ServiceType, service.ImplementationType, ServiceLifetime.Transient));
                    } else
                    {
                        childServices.Insert(0, new ServiceDescriptor(service.ServiceType, (IServiceProvider p) => provider.GetRequiredService(service.ServiceType), ServiceLifetime.Transient));
                    }
                }

                configure(childServices);
                childProvider = new OwnedProvider(childServices.BuildServiceProvider());

                // Should only be done once!! This prevents multiple initializations
                hasBeenRouted = true;
            }

            // NOTE: Order is important here. We want to check the child
            //       provider before the parent
            if (childProvider.IsDisposed)
                throw new InvalidOperationException("Child provider has been disposed.");

            IServiceProvider coalescedProvider;
            coalescedProvider = new CoalescedServiceProvider(childProvider, provider);

            return ActivatorUtilities.CreateInstance(coalescedProvider, implementationType);
        };
        services.Add(new ServiceDescriptor(serviceType, factory, lifetime));

        // NOTE: The provider is added to the collection to allow
        //       for the lifetime to be handled by the host (i.e. ASP.NET Core)
        // NOTE: The factory is required to ensure it gets disposed.
        //       Using the instance directly won't dispose at the end of the parent's life.
        
        services.AddSingleton(p => childProvider);

        return services;
    }
}