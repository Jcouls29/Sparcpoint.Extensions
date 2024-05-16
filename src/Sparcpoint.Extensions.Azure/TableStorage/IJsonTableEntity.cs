using Azure;
using Azure.Data.Tables;

namespace Sparcpoint.Extensions.Azure;

public interface IJsonTableEntity
{
    string? PartitionKey { get; set; }
    string? RowKey { get; set; }
    DateTimeOffset? Timestamp { get; set; }
    ETag ETag { get; set; }

    internal ITableEntity GetValue();
    internal void SetValue(TableEntity entity);
}
