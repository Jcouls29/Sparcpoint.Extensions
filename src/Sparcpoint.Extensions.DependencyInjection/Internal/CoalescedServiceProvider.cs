namespace Microsoft.Extensions.DependencyInjection;

internal class CoalescedServiceProvider : IServiceProvider
{
    private readonly IServiceProvider[] _OwnedProviders;

    public CoalescedServiceProvider(params IServiceProvider[] ownedProviders)
    {
        _OwnedProviders = ownedProviders?.ToArray() ?? throw new ArgumentNullException(nameof(ownedProviders));
        if (_OwnedProviders.Length == 0)
            throw new ArgumentException("At least one service provider is required.", nameof(ownedProviders));
        if (_OwnedProviders.Any(s => s is null))
            throw new NullReferenceException("At least one of the services provided is null.");
    }

    public object? GetService(Type serviceType)
    {
        foreach(var provider in _OwnedProviders)
        {
            object? foundService = provider.GetService(serviceType);
            if (foundService != null)
                return foundService;
        }

        return null;
    }
}
