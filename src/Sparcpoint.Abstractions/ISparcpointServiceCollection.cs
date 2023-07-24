using Microsoft.Extensions.DependencyInjection;

namespace Sparcpoint.Abstractions
{
    public interface ISparcpointServiceCollection
    {
        IServiceCollection Services { get; }
    }

}
