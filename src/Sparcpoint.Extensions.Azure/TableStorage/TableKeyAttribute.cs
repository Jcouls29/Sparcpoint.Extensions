namespace Sparcpoint.Extensions.Azure;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class TableKeyAttribute : Attribute
{
    public TableKeyAttribute(string partitionKeyFormat, string rowKeyFormat)
    {
        PartitionKeyFormat = partitionKeyFormat;
        RowKeyFormat = rowKeyFormat;
    }

    public string PartitionKeyFormat { get; }
    public string RowKeyFormat { get; }
}
