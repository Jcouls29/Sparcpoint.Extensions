﻿/*
The MIT License (MIT)

Copyright (c) 2015 Kristian Hellang
Copyright (c) 2024 Justin Coulston

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

// ref: https://github.com/khellang/Scrutor
// Code has been modified from original source, however, it is heavily influenced by Scrutor

using Sparcpoint.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class SparcpointServiceCollectionExtensions
{
    private static ServiceDescriptor WithImplementationFactory(this ServiceDescriptor descriptor, Func<IServiceProvider, object> implementationFactory)
        => new(descriptor.ServiceType, implementationFactory, descriptor.Lifetime);

    private static ServiceDescriptor WithServiceType(this ServiceDescriptor descriptor, Type serviceType) => descriptor switch
    {
        { ImplementationType: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationType, descriptor.Lifetime),
        { ImplementationFactory: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationFactory, descriptor.Lifetime),
        { ImplementationInstance: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationInstance),
        _ => throw new ArgumentException($"No implementation factory or instance or type found for {descriptor.ServiceType}.", nameof(descriptor))
    };

    private static bool TryDecorateClosed(IServiceCollection services, Type serviceType, Type decoratorType)
    {
        var decorated = false;
        for (var i = 0; i < services.Count; i++)
        {
            if (services[i].ServiceType == serviceType)
            {
                var descriptor = services[i];

                if (descriptor.ServiceType is DecoratedType)
                    continue;

                var decoratedTypeEx = new DecoratedType(descriptor.ServiceType);

                var factory = (IServiceProvider sp) =>
                {
                    var instance = sp.GetRequiredService(decoratedTypeEx);
                    return ActivatorUtilities.CreateInstance(sp, decoratorType, instance);
                };

                services.Add(descriptor.WithServiceType(decoratedTypeEx));
                services[i] = descriptor.WithImplementationFactory(factory);

                decorated = true;
            }
        }

        return decorated;
    }

    private static bool TryDecorateOpen(IServiceCollection services, Type serviceType, Type decoratorType)
    {
        var decorated = false;
        for (var i = 0; i < services.Count; i++)
        {
            var descriptor = services[i];

            if (descriptor.ServiceType is DecoratedType)
                continue;

            if (!CanDecorateOpen(serviceType, decoratorType, descriptor.ServiceType))
                continue;

            var decoratedTypeEx = new DecoratedType(descriptor.ServiceType);
            var genericArguments = decoratedTypeEx.GetGenericArguments();
            var closedDecorator = decoratorType.MakeGenericType(genericArguments);

            var factory = (IServiceProvider sp) =>
            {
                var instance = sp.GetRequiredService(decoratedTypeEx);
                return ActivatorUtilities.CreateInstance(sp, closedDecorator, instance);
            };

            services.Add(descriptor.WithServiceType(decoratedTypeEx));
            services[i] = descriptor.WithImplementationFactory(factory);

            decorated = true;
        }

        return decorated;
    }

    private static bool CanDecorateOpen(Type decoratorServiceType, Type decoratorType, Type targetServiceType)
    {
        return targetServiceType.IsGenericType
            && !targetServiceType.IsGenericTypeDefinition
            && targetServiceType.GetGenericTypeDefinition() == decoratorServiceType.GetGenericTypeDefinition()
            && (decoratorType is null || targetServiceType.HasCompatibleGenericArguments(decoratorType));
    }

    private static bool HasCompatibleGenericArguments(this Type type, Type genericTypeDefinition)
    {
        var genericArguments = type.GetGenericArguments();
        try
        {
            _ = genericTypeDefinition.MakeGenericType(genericArguments);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
