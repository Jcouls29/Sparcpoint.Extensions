using Azure;
using Azure.Data.Tables;

namespace Sparcpoint.Extensions.Azure;

public interface IJsonTableEntity
{
    string? PartitionKey { get; set; }
    string? RowKey { get; set; }
    DateTimeOffset? Timestamp { get; set; }
    ETag ETag { get; set; }

    ITableEntity GetValue();
    void SetValue(TableEntity entity);
}
