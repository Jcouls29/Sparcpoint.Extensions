namespace Sparcpoint.Extensions.Multitenancy;

public sealed class TenantContext<TTenant> : IDisposable
{
    public TenantContext(TTenant tenant)
    {
        Ensure.ArgumentNotNull(tenant);

        Tenant = tenant;
        Properties = new Dictionary<string, object>();
        Id = Guid.NewGuid().ToString();
    }

    public string Id { get; }
    public TTenant Tenant { get; }
    public IDictionary<string, object> Properties { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool disposed = false;
    private void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing)
        {
            foreach(var prop in Properties)
            {
                if (prop.Value is IDisposable disposableProperty)
                    TryDispose(disposableProperty);
            }

            if (Tenant is IDisposable disposableTenant)
                TryDispose(disposableTenant);
        }

        disposed = true;
    }

    private void TryDispose(IDisposable? disposable)
    {
        try
        {
            disposable?.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Do Nothing
        }
    }
}