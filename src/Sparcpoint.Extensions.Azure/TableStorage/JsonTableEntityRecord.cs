using Azure;
using Azure.Data.Tables;

namespace Sparcpoint.Extensions.Azure;

public abstract record JsonTableEntityRecord : IJsonTableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    ITableEntity IJsonTableEntity.GetValue()
        => JsonTableEntityHelpers.GetValue(this);

    void IJsonTableEntity.SetValue(TableEntity entity)
        => JsonTableEntityHelpers.SetValue(this, entity);
}
