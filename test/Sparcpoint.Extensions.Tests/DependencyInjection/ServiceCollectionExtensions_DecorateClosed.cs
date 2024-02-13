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
using static Sparcpoint.Extensions.Tests.ServiceCollectionExtensions_DecorateOpen;

namespace Sparcpoint.Extensions.Tests;

// ref: https://github.com/khellang/Scrutor/blob/master/test/Scrutor.Tests/DecorationTests.cs
// A portion of this code is copyrighted
// Copyright (c) 2015 Kristian Hellang
public class ServiceCollectionExtensions_DecorateClosed
{
    [Fact]
    public void Can_decorate_type()
    {
        var instance = Build<IInterface>(s =>
        {
            s.AddSingleton<IInterface, Instance>();
            s.Decorate<IInterface, DecoratedInstance>();
        });

        var decorated = Assert.IsType<DecoratedInstance>(instance);
        Assert.IsType<Instance>(decorated.InnerService);
    }

    [Fact]
    public void Can_decorate_multiple_levels()
    {
        var instance = Build<IInterface>(s =>
        {
            s.AddSingleton<IInterface, Instance>();
            s.Decorate<IInterface, DecoratedInstance>();
            s.Decorate<IInterface, DecoratedInstance>();
        });

        var outer = Assert.IsType<DecoratedInstance>(instance);
        var inner = Assert.IsType<DecoratedInstance>(outer.InnerService);
        Assert.IsType<Instance>(inner.InnerService);
    }

    [Fact]
    public void Can_decorate_different_implementations()
    {
        var instance = Build<IEnumerable<IInterface>>(s =>
        {
            s.AddSingleton<IInterface, Instance>();
            s.AddSingleton<IInterface, OtherInstance>();

            s.Decorate<IInterface, DecoratedInstance>();
        });

        Assert.Equal(2, instance.Count());
        Assert.All(instance, x => Assert.IsType<DecoratedInstance>(x));
        Assert.Contains(instance, x => Assert.IsType<DecoratedInstance>(x).InnerService is Instance);
        Assert.Contains(instance, x => Assert.IsType<DecoratedInstance>(x).InnerService is OtherInstance);
    }

    [Fact]
    public void Can_decorate_existing_instance()
    {
        var existing = new Instance();

        var instance = Build<IInterface>(s =>
        {
            s.AddSingleton<IInterface>(existing);
            s.Decorate<IInterface, DecoratedInstance>();
        });

        var decorator = Assert.IsType<DecoratedInstance>(instance);
        var innerService = Assert.IsType<Instance>(decorator.InnerService);

        Assert.Same(existing, innerService);
    }

    [Fact]
    public void Can_inject_services_into_decorated_type()
    {
        var provider = Build(s =>
        {
            s.AddSingleton<Service>();
            s.AddSingleton<IInterface, Instance>();

            s.Decorate<IInterface, DecoratedInstance>();
        });

        var innerService = provider.GetRequiredService<Service>();
        var instance = provider.GetRequiredService<IInterface>();

        var decorator = Assert.IsType<DecoratedInstance>(instance);
        var decorated = Assert.IsType<Instance>(decorator.InnerService);

        Assert.Same(innerService, decorated.Service);
    }

    [Fact]
    public void Can_inject_services_into_decorator_type()
    {
        var provider = Build(s =>
        {
            s.AddSingleton<Service>();
            s.AddSingleton<IInterface, Instance>();

            s.Decorate<IInterface, DecoratedInstance>();
        });

        var innerService = provider.GetRequiredService<Service>();
        var instance = provider.GetRequiredService<IInterface>();

        var decorator = Assert.IsType<DecoratedInstance>(instance);
        var decorated = Assert.IsType<Instance>(decorator.InnerService);

        Assert.Same(innerService, decorator.Service);
    }

    [Fact]
    public void Disposable_services_get_disposed()
    {
        DisposableService instance;
        DisposableDecoratorService decorator;
        using (var provider = Build(s =>
        {
            s.AddSingleton<IInterface, DisposableService>();
            s.Decorate<IInterface, DisposableDecoratorService>();
        }))
        {
            decorator = Assert.IsType<DisposableDecoratorService>(provider.GetRequiredService<IInterface>());
            instance = Assert.IsType<DisposableService>(decorator.InnerService);

            Assert.False(decorator.IsDisposed);
            Assert.False(instance.IsDisposed);
        }

        Assert.True(decorator.IsDisposed);
        Assert.True(instance.IsDisposed);
    }

    [Fact]
    public void Services_with_same_type_are_only_decorated_once()
    {
        var instances = Build<IEnumerable<IInterface>>(s =>
        {
            s.AddSingleton<IInterface, OtherInstance>();
            s.AddSingleton<IInterface, OtherInstance2>();
            s.Decorate<IInterface, DecoratedInstance>();
        });

        foreach(var instance in instances)
        {
            Assert.IsType<DecoratedInstance>(instance);
        }

        Assert.Contains(instances, (i) => Assert.IsType<DecoratedInstance>(i).InnerService is OtherInstance);
        Assert.Contains(instances, (i) => Assert.IsType<DecoratedInstance>(i).InnerService is OtherInstance2);
    }

    [Fact]
    public void Can_decorate_concrete_classes()
    {
        var instance = Build<Service>(s =>
        {
            s.AddSingleton<Service>();
            s.Decorate<Service, Service2>();
        });

        var instance2 = Assert.IsType<Service2>(instance);
        Assert.IsType<Service>(instance2.InnerService);
    }

    [Fact]
    public void Decorating_non_registered_service_throws()
    {
        Assert.Throws<MissingTypeRegistrationException>(() => Build(s => s.Decorate<IInterface, DecoratedInstance>()));
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

    public interface IInterface { }
    public class Instance : IInterface
    {
        public Instance(Service? service = null)
        {
            Service = service;
        }

        public Service? Service { get; }
    }
    public class Service { }

    public class Service2 : Service
    {
        public Service2(Service service)
        {
            InnerService = service;
        }

        public Service InnerService { get; }
    }

    public class OtherInstance : IInterface { }
    public class OtherInstance2 : IInterface { }
    public class DecoratedInstance : IInterface
    {
        public DecoratedInstance(IInterface innerService, Service? service = null)
        {
            InnerService = innerService;
            Service = service;
        }

        public IInterface InnerService { get; }
        public Service? Service { get; }
    }

    public class DisposableService : IInterface, IDisposable
    {
        public bool IsDisposed { get; set; } = false;
        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public class DisposableDecoratorService : DisposableService
    {
        public DisposableDecoratorService(IInterface innerService)
        {
            InnerService = innerService;
        }

        public IInterface InnerService { get; }
    }
}


