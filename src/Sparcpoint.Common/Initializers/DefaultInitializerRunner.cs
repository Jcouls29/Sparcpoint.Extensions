namespace Sparcpoint.Common.Initializers;

public class DefaultInitializerRunner : IInitializerRunner
{
    private readonly IEnumerable<IInitializer> _Initializers;
    private readonly List<IInitializer> _HasRun;

    public DefaultInitializerRunner(IEnumerable<IInitializer> initializers)
    {
        _Initializers = initializers;
        _HasRun = new List<IInitializer>();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    private bool _disposed = false;
    private async ValueTask DisposeAsync(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
        {
            if (_Initializers == null || _HasRun.Count == 0)
                return;

            foreach(var ran in _HasRun)
            {
                if (ran is IDisposable disposable)
                    DisposeInitializer(disposable);
                if (ran is IAsyncDisposable asyncDisposable)
                    await DisposeInitializer(asyncDisposable);
            }
        }
    }

    public async Task RunAsync()
    {
        if (_Initializers == null)
            return;

        foreach(var i in _Initializers)
        {
            await RunAsync(i);
        }
    }

    private async Task RunAsync(IInitializer initializer)
    {
        try
        {
            await initializer.InitializeAsync();
            _HasRun.Add(initializer);
        } catch
        {
            // TODO: Do Nothing (yet)
        }
    }

    private void DisposeInitializer(IDisposable disposable)
    {
        try
        {
            disposable.Dispose();
        } catch
        {
            // TODO: Do Nothing (yet)
        }
    }

    private async Task DisposeInitializer(IAsyncDisposable disposable)
    {
        try
        {
            await disposable.DisposeAsync();
        }
        catch
        {
            // TODO: Do Nothing (yet)
        }
    }
}