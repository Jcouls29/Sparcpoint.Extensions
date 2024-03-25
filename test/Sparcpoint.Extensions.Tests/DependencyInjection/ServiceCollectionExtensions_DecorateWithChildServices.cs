namespace Sparcpoint.Extensions.Tests;

public class ServiceCollectionExtensions_DecorateWithChildServices
{
    /* Scenario:
     * Decorated class will pick up parent container service
     * if registered
     */
    [Fact]
    public void When_child_and_parent_provide_child_instance_is_used()
    {
        using var provider = Build(s =>
        {
            s.AddSingleton<IService, BaseService>();
            s.AddSingleton(new OtherService("Parent"));
            s.DecorateWithChildServices<IService, DecoratedService>(child =>
            {
                child.AddSingleton(new OtherService("Child"));
            });
        });

        var service = provider.GetService<IService>();
        Assert.NotNull(service);
        var decoratedService = Assert.IsType<DecoratedService>(service);
        Assert.IsType<BaseService>(decoratedService.Service);
        var otherService = Assert.IsType<OtherService>(decoratedService.Other);
        Assert.Equal("Child", otherService.Value);
    }

    [Fact]
    public void When_child_provider_missing_service_pulls_from_parent()
    {
        using var provider = Build(s =>
        {
            s.AddSingleton<IService, BaseService>();
            s.AddSingleton(new OtherService("Parent"));
            s.DecorateWithChildServices<IService, DecoratedService>(child => { });
        });

        var service = provider.GetService<IService>();
        Assert.NotNull(service);
        var decoratedService = Assert.IsType<DecoratedService>(service);
        Assert.IsType<BaseService>(decoratedService.Service);
        var otherService = Assert.IsType<OtherService>(decoratedService.Other);
        Assert.Equal("Parent", otherService.Value);
    }

    private ServiceProvider Build(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }

    public class OtherService
    {
        public OtherService(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    public interface IService { }
    public class BaseService : IService { }
    public class DecoratedService : IService
    {
        public DecoratedService(IService service, OtherService other)
        {
            Service = service;
            Other = other;
        }

        public IService Service { get; }
        public OtherService Other { get; }
    }
}

