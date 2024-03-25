/*
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
    public static IServiceCollection Decorate<TService, TDecoration>(this IServiceCollection services) where TDecoration : TService
        => Decorate(services, typeof(TService), typeof(TDecoration));

    // Alias for DecorateWithChildServices
    public static IServiceCollection Decorate<TService, TDecoration>(this IServiceCollection services, Action<IServiceCollection> configureChildServices, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        => DecorateWithChildServices<TService, TDecoration>(services, configureChildServices, serviceLifetime);

    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        bool decorated;
        if (serviceType.IsGenericTypeDefinition)
            decorated = TryDecorateOpen(services, serviceType, decoratorType);
        else
            decorated = TryDecorateClosed(services, serviceType, decoratorType);

        if (!decorated)
            throw new MissingTypeRegistrationException(serviceType);

        return services;
    }
}
