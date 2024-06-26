using Microsoft.Extensions.DependencyInjection;

namespace Sparcpoint.Extensions.Tests;

public class ServiceCollectionExtensions_ChildServices
{
    /* Scenario: 
     * Two parent services are registered at the top
     * level with different child services. When provided
     * they contain different instances of the services
     */
    [Fact]
    public void Parent_services_use_different_instances()
    {
        using var provider = Build((services) =>
        {
            services.WithChildServices<ParentService>(child =>
            {
                child.AddSingleton(new ChildService("A"));
            });
            services.WithChildServices<ParentService>(child =>
            {
                child.AddSingleton(new ChildService("B"));
            });
        });

        var parents = provider.GetServices<ParentService>().ToArray();
        Assert.Equal(2, parents.Length);
        Assert.NotSame(parents[0].Child, parents[1].Child);
        Assert.Contains(parents, (p) => p.Child.Name == "A");
        Assert.Contains(parents, (p) => p.Child.Name == "B");
    }

    /* Scenario:
     * Child services are scoped to the parent only and
     * cannot be provided with the top provider
     */
    [Fact]
    public void Child_services_cannot_be_provided_from_parent_provider()
    {
        using var provider = Build((services) =>
        {
            services.WithChildServices<ParentService>(child => child.AddSingleton(new ChildService("C")));
        });

        var foundService = provider.GetService<ChildService>();
        Assert.Null(foundService);
    }

    /* Scenario:
     * We want to ensure that when a singleton is used for the child
     * service, the same instance is returned with each provide of 
     * a transient parent service
     */
    [Fact]
    public void Given_child_service_is_singleton_they_are_the_same_instances()
    {
        using var provider = Build((services) =>
        {
            services.WithChildServices<ParentService>(child => child.AddSingleton<ChildService>(), lifetime: ServiceLifetime.Transient);
        });

        var foundService1 = provider.GetService<ParentService>();
        var foundService2 = provider.GetService<ParentService>();

        Assert.NotNull(foundService1);
        Assert.NotNull(foundService2);
        Assert.Same(foundService1.Child, foundService2.Child);
    }

    /* Scenario:
     * We want to ensure that when a transient is used for the child
     * service, a different instance is returned with each provide
     * of a transient parent service
     */
    [Fact]
    public void Given_child_service_is_transient_they_are_not_the_same_instances()
    {
        using var provider = Build((services) =>
        {
            services.WithChildServices<ParentService>(child => child.AddTransient<ChildService>(), lifetime: ServiceLifetime.Transient);
        });

        var foundService1 = provider.GetService<ParentService>();
        var foundService2 = provider.GetService<ParentService>();

        Assert.NotNull(foundService1);
        Assert.NotNull(foundService2);
        Assert.NotSame(foundService1.Child, foundService2.Child);
    }

    /* Scenario:
     * We want to ensure that when the parent is disposed, the child
     * providers are also disposed.
     */
    [Fact]
    public void When_parent_provider_is_disposed_all_child_providers_are_disposed()
    {
        OwnedProvider _ownedProvider;

        using (var provider = Build((services) =>
        {
            services.WithChildServices<ParentService>(child => child.AddTransient<ChildService>(), lifetime: ServiceLifetime.Transient);
        }))
        {
            _ownedProvider = provider.GetRequiredService<OwnedProvider>();
            Assert.NotNull(_ownedProvider);
            Assert.False(_ownedProvider.IsDisposed);
        }

        Assert.True(_ownedProvider.IsDisposed);
    }

    /* Scenario:
     * When a child service does not have the service registered,
     * it should get the service from the parent instead
     */
    [Fact]
    public void When_child_provider_missing_service_pulls_from_parent()
    {
        using var provider = Build((services) =>
        {
            services.AddSingleton<ChildService>();
            services.WithChildServices<GrandParentService>(child => child.AddSingleton<ParentService>());
        });

        var found = provider.GetService<GrandParentService>();
        Assert.NotNull(found);
        Assert.NotNull(found.Parent);
        Assert.NotNull(found.Parent.Child);
    }

    /* Scenario:
     * The child service should always provide before the parent
     */
    [Fact]
    public void When_child_and_parent_provide_child_instance_is_used()
    {
        using var provider = Build((services) =>
        {
            services.AddSingleton<ChildService>();
            services.WithChildServices<GrandParentService>(child => child.AddSingleton<ParentService>().AddSingleton<ChildService>());
        });

        var rootGrandparent = provider.GetService<GrandParentService>();
        Assert.NotNull(rootGrandparent);

        var rootChild = provider.GetService<ChildService>();
        Assert.NotNull(rootChild);

        Assert.NotSame(rootGrandparent.Parent.Child, rootChild);
    }

    [Fact]
    public void When_implementation_is_not_service_it_provides()
    {
        using var provider = Build((services) =>
        {
            services.WithChildServices<IParent, ParentService>(child => child.AddSingleton<ChildService>());
        });

        var parent = provider.GetService<IParent>();
        Assert.NotNull(parent);
        Assert.NotNull(parent.Child);
    }

    [Fact]
    public void Child_can_decorate_parent_provider_services()
    {
        var provider = Build(s =>
        {
            s.AddSingleton<ChildService>();
            s.WithChildServices<IParent, ParentService>(s2 => s2.Decorate<ChildService, DecoratedChildService>());
        });

        var parent = provider.GetService<IParent>();
        var service = Assert.IsType<ParentService>(parent);
        var decoratedChild = Assert.IsType<DecoratedChildService>(service.Child);
        Assert.IsType<ChildService>(decoratedChild.Child);
    }

    [Fact]
    public void Parent_provider_service_does_not_get_decorated_when_provided_from_parent_provider()
    {
        var provider = Build(s =>
        {
            s.AddSingleton<ChildService>();
            s.WithChildServices<IParent, ParentService>(s2 => s2.Decorate<ChildService, DecoratedChildService>());
        });

        var service = provider.GetService<ChildService>();
        Assert.IsType<ChildService>(service);
        Assert.IsNotType<DecoratedChildService>(service);
    }

    [Fact]
    public void Parent_can_be_decorated()
    {
        var provider = Build(s =>
        {
            s.WithChildServices<IParent, ParentService>(child => child.AddSingleton<ChildService>());
            s.Decorate<IParent, DecoratedParent>();
        });

        var parent = provider.GetService<IParent>();
        var decorated = Assert.IsType<DecoratedParent>(parent);
        var inner = Assert.IsType<ParentService>(decorated.InnerParent);
        Assert.IsType<ChildService>(inner.Child);
    }

    [Fact]
    public void Concrete_service_can_be_injected_into_concrete_service()
    {
        var provider = Build(s =>
        {
            s.WithChildServices<NoInterfaceClass>(child => child.AddSingleton<DtoClass>());
        });

        var service = provider.GetService<NoInterfaceClass>();
        Assert.NotNull(service);
    }

    [Fact]
    public void Instance_child_service_provides()
    {
        var provider = Build(s =>
        {
            s.WithChildServices<NoInterfaceClass>(child => child.AddSingleton(new DtoClass { }));
        });

        var service = provider.GetService<NoInterfaceClass>();
        Assert.NotNull(service);
    }

    private ServiceProvider Build(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }

    public class GrandParentService(ParentService parent)
    {
        public ParentService Parent { get; } = parent;
    }

    public class ParentService(ChildService child) : IParent
    {
        public ChildService Child { get; } = child;
    }

    public class ChildService(string name = "")
    {
        public string Name { get; } = name;
    }

    public class DecoratedChildService(ChildService service) : ChildService
    {
        public ChildService Child { get; } = service;
    }

    public interface IParent
    {
        ChildService Child { get; }
    }

    public interface IParent<T>
    {
        IChild<T> Child { get; }
    }
    public class OpenParent<T> : IParent<T>
    {
        public OpenParent(IChild<T> child)
        {
            Child = child;
        }

        public IChild<T> Child { get; }
    }

    public class DecoratedParent : IParent
    {
        public DecoratedParent(IParent parent)
        {
            InnerParent = parent;
            Child = parent.Child;
        }

        public IParent InnerParent { get; }
        public ChildService Child { get; }
    }

    public class DecoratedParent<T> : IParent<T>
    {
        public DecoratedParent(IParent<T> parent)
        {
            InnerParent = parent;
            Child = parent.Child;
        }

        public IParent<T> InnerParent { get; }
        public IChild<T> Child { get; }
    }

    public interface IChild<T> { }
    public class OpenChild<T> : IChild<T> { }

    public class NoInterfaceClass
    {
        private readonly DtoClass _Dto;

        public NoInterfaceClass(DtoClass dto)
        {
            _Dto = dto ?? throw new Exception();
        }
    }

    public class DtoClass
    {
        
    }
}


