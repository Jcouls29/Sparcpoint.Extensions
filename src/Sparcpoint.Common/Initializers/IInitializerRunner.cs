namespace Sparcpoint.Common.Initializers;

public interface IInitializerRunner : IAsyncDisposable
{
    Task RunAsync();
}
