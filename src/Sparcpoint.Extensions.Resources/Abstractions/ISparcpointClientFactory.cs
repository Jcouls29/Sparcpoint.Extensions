namespace Sparcpoint.Extensions.Resources;

public interface ISparcpointClientFactory
{
    ISparcpointClient Create(string accountId);
}