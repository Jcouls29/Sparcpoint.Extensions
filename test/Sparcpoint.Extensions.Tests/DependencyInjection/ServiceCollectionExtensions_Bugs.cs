using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Tests.DependencyInjection;

public class ServiceCollectionExtensions_Bugs
{
    // ArgumentException: Open generic service type 'Microsoft.Extensions.Options.IOptions`1[TOptions]`
    // requires registering an open generic implementation type. (Parameter 'descriptors')
    [Fact]
    public void Bug_01()
    {
        var provider = Build(s =>
        {
            s.WithChildServices<Consumer>(c => c.AddSingleton<IGenerator, GeneratorClass>());
            s.AddMemoryCache();
            s.AddControllers();
            s.AddAuthentication();
            s.AddCors();
        });

        var consumer = provider.GetRequiredService<Consumer>();
        Assert.NotNull(consumer);
    }

    private interface IGenerator { }
    private class GeneratorClass : IGenerator { }

    private class Consumer
    {
        public Consumer(IGenerator generator) { }
    }

    private ServiceProvider Build(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }
}
