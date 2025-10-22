namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Internal class used to wrap a service provider with IDisposable
/// so the parent can dispose, at the end of its useful lifetime
/// </summary>
internal class OwnedProvider : IServiceProvider, IDisposable
{
    private readonly IServiceProvider? _Provider;

    public OwnedProvider()
    {
        _Provider = null;
    }

    public OwnedProvider(IServiceProvider provider)
    {
        _Provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public object? GetService(Type serviceType)
        => _Provider?.GetService(serviceType);

    private volatile bool _IsDisposed = false;
    internal bool IsDisposed => _IsDisposed;

    public void Dispose()
    {
        if (_IsDisposed)
            return;

        lock (this)
        {
            if (_IsDisposed)
                return;

            (_Provider as IDisposable)?.Dispose();
            _IsDisposed = true;
        }
    }
}
