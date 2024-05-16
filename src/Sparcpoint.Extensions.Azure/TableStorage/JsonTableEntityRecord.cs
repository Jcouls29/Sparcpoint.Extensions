using Azure;
using Azure.Data.Tables;

namespace Sparcpoint.Extensions.Azure;

public abstract record JsonTableEntityRecord
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    internal ITableEntity GetValue()
        => JsonTableEntityHelpers.GetValue(this);

    internal void SetValue(TableEntity entity)
        => JsonTableEntityHelpers.SetValue(this, entity);
}
