using Microsoft.Extensions.DependencyInjection;
using Sparcpoint.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class SparcpointServiceCollectionExtensions
{
    public static IServiceCollection DecorateWithChildServices<TService, TDecoration>(this IServiceCollection services, Action<IServiceCollection> configure, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        return DecorateWithChildServices(services, typeof(TService), typeof(TDecoration), configure, lifetime);
    }

    private static IServiceCollection DecorateWithChildServices(this IServiceCollection services, Type serviceType, Type implementationType, Action<IServiceCollection> configure, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        if (serviceType.IsGenericTypeDefinition)
            throw new InvalidOperationException("Open generics are not supported.");

        bool hasBeenRouted = false;

        // NOTE: This approach allows for many nested coalesced providers.
        //       This should not cause any issues.
        var childServices = new ServiceCollection();
        OwnedProvider childProvider = new OwnedProvider();

        Func<IServiceProvider, object, object> factory = (IServiceProvider provider, object decoratedInstance) =>
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
                    }
                    else
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

            return ActivatorUtilities.CreateInstance(coalescedProvider, implementationType, decoratedInstance);
        };

        for (var i = 0; i < services.Count; i++)
        {
            if (services[i].ServiceType == serviceType)
            {
                var descriptor = services[i];
                if (descriptor.ServiceType is DecoratedType)
                    continue;
                var decoratedTypeEx = new DecoratedType(descriptor.ServiceType);

                // TODO: DecorateFactory
                var decorateFactory = (IServiceProvider sp) =>
                {
                    var instance = sp.GetRequiredService(decoratedTypeEx);
                    return factory(sp, instance);
                };

                services.Add(descriptor.WithServiceType(decoratedTypeEx));
                services[i] = descriptor.WithImplementationFactory(decorateFactory);
            }
        }

        // NOTE: The provider is added to the collection to allow
        //       for the lifetime to be handled by the host (i.e. ASP.NET Core)
        // NOTE: The factory is required to ensure it gets disposed.
        //       Using the instance directly won't dispose at the end of the parent's life.

        services.AddSingleton(p => childProvider);

        return services;
    }
}
