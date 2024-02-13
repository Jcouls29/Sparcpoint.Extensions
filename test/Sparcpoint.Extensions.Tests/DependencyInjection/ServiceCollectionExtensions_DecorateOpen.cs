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

using Microsoft.Extensions.DependencyInjection;
using static Sparcpoint.Extensions.Tests.ServiceCollectionExtensions_DecorateClosed;

namespace Sparcpoint.Extensions.Tests;

public class ServiceCollectionExtensions_DecorateOpen
{
    [Fact]
    public void Can_decorate_open_generic_type_based_on_interface()
    {
        var instance = Build<IOpenGeneric<string, long>>(s =>
        {
            s.AddSingleton<IOpenGeneric<string, long>, OpenGenericInstance>();
            s.Decorate(typeof(IOpenGeneric<,>), typeof(DecoratedOpenGenericInstance<,>));
        });

        var decorator = Assert.IsType<DecoratedOpenGenericInstance<string, long>>(instance);
        Assert.IsType<OpenGenericInstance>(decorator.InnerService);
    }

    [Fact]
    public void Can_decorate_open_generic_type_based_on_class()
    {
        var instance = Build<OpenGenericInstance<string, long>>(s =>
        {
            s.AddSingleton<OpenGenericInstance<string, long>, OpenGenericImplementation>();
            s.Decorate(typeof(OpenGenericInstance<,>), typeof(DecoratedOpenGenericInstanceClass<,>));
        });

        var decorator = Assert.IsType<DecoratedOpenGenericInstanceClass<string, long>>(instance);
        Assert.IsType<OpenGenericImplementation>(decorator.InnerService);
    }

    private T? Build<T>(Action<IServiceCollection> configure)
    {
        var provider = Build(configure);
        return provider.GetService<T>();
    }

    private ServiceProvider Build(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }

    public interface IOpenGeneric<T1, T2> { }
    public class OpenGenericInstance : IOpenGeneric<string, long> { }
    public class DecoratedOpenGenericInstance<T1, T2> : IOpenGeneric<T1, T2>
    {
        public DecoratedOpenGenericInstance(IOpenGeneric<T1, T2> innerService)
        {
            InnerService = innerService;
        }

        public IOpenGeneric<T1, T2> InnerService { get; }
    }
    public class OpenGenericInstance<T1, T2> { }
    public class OpenGenericImplementation : OpenGenericInstance<string, long> { }
    public class DecoratedOpenGenericInstanceClass<T1, T2> : OpenGenericInstance<T1, T2>
    {
        public DecoratedOpenGenericInstanceClass(OpenGenericInstance<T1, T2> innerService)
        {
            InnerService = innerService;
        }

        public OpenGenericInstance<T1, T2> InnerService { get; }
    }
}


