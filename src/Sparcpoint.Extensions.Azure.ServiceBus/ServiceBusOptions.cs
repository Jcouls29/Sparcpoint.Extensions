namespace Sparcpoint.Extensions.Azure.ServiceBus
{
    public class ServiceBusOptions
    {
        public string ConnectionString { get; set; } = string.Empty;

        public bool CreateTopicIfNotExists { get; set; } = false;
        public string ResourceId { get; set; } = null;
    }
}
