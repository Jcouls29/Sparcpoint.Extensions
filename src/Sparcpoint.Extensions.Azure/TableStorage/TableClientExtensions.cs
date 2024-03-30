using SmartFormat;
using Sparcpoint.Extensions.Azure;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Azure.Data.Tables;

public static class TableClientExtensions
{
    public static async Task<Azure.Response> UpsertEntityAsync<T>(this TableClient client, T entity, TableUpdateMode updateMode = TableUpdateMode.Merge, CancellationToken cancelToken = default) where T : JsonTableEntity
    {
        return await client.UpsertEntityAsync(entity.GetValue(), updateMode, cancelToken);
    }

    public static async Task<Azure.Response> UpdateEntityAsync<T>(this TableClient client, T entity, ETag etag, TableUpdateMode updateMode = TableUpdateMode.Merge, CancellationToken cancelToken = default) where T : JsonTableEntity
    {
        return await client.UpdateEntityAsync(entity.GetValue(), etag, updateMode, cancelToken);
    }

    public static async Task<Azure.Response> AddEntityAsync<T>(this TableClient client, T entity, CancellationToken cancelToken = default) where T : JsonTableEntity
    {
        return await client.AddEntityAsync(entity.GetValue(), cancelToken);
    }

    public static async IAsyncEnumerable<T> QueryAsync<T>(
        this TableClient client,
        string filter,
        int? maxPerPage = null,
        IEnumerable<string>? select = null,
        [EnumeratorCancellation]
        CancellationToken cancelToken = default) where T : JsonTableEntity, new()
    {
        await foreach (var entity in client.QueryAsync<TableEntity>(filter, maxPerPage, select, cancelToken))
        {
            T instance = new();
            instance.SetValue(entity);

            yield return instance;
        }
    }

    public static IAsyncEnumerable<T> PartitionQueryAsync<T>(
        this TableClient client,
        object? parameters = null,
        int? maxPerPage = null,
        IEnumerable<string>? select = null,
        CancellationToken cancelToken = default) where T : JsonTableEntity, new()
    {
        var attr = typeof(T).GetCustomAttribute<TableKeyAttribute>();
        if (attr == null)
            throw new InvalidOperationException("This operation requires the entity to be decorated with a TableKeyAttribute.");

        var partitionKey = attr.PartitionKeyFormat;
        if (parameters != null)
            partitionKey = Smart.Format(partitionKey, parameters);

        return PartitionQueryAsync<T>(client, partitionKey, maxPerPage, select, cancelToken);
    }

    public static async IAsyncEnumerable<T> PartitionQueryAsync<T>(
        this TableClient client,
        string partitionKey,
        int? maxPerPage = null,
        IEnumerable<string>? select = null,
        [EnumeratorCancellation]
        CancellationToken cancelToken = default) where T : JsonTableEntity, new()
    {
        await foreach (var entity in client.QueryAsync<TableEntity>(f => f.PartitionKey == partitionKey, maxPerPage, select, cancelToken))
        {
            T instance = new();
            instance.SetValue(entity);

            yield return instance;
        }
    }

    public static async Task<T?> GetEntityIfExistsAsync<T>(
        this TableClient client,
        object? parameters = null,
        IEnumerable<string>? select = null,
        CancellationToken cancelToken = default) where T : JsonTableEntity, new()
    {
        var attr = typeof(T).GetCustomAttribute<TableKeyAttribute>();
        if (attr == null)
            throw new InvalidOperationException("This operation requires the entity to be decorated with a TableKeyAttribute.");

        var partitionKey = attr.PartitionKeyFormat;
        if (parameters != null)
            partitionKey = Smart.Format(partitionKey, parameters);

        var rowKey = attr.RowKeyFormat;
        if (parameters != null)
            rowKey = Smart.Format(rowKey, parameters);

        return await GetEntityIfExistsAsync<T>(client, partitionKey, rowKey, select, cancelToken);
    }

    public static async Task<T?> GetEntityIfExistsAsync<T>(
        this TableClient client,
        string partitionKey,
        string rowKey,
        IEnumerable<string>? select = null,
        CancellationToken cancelToken = default) where T : JsonTableEntity, new()
    {
        var entity = await client.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey, select, cancelToken);
        if (entity == null || !entity.HasValue || entity.Value == null)
            return null;

        T instance = new();
        instance.SetValue(entity.Value);

        return instance;
    }

    public static async Task<T?> GetEntityAsync<T>(
        this TableClient client,
        object? parameters = null,
        IEnumerable<string>? select = null,
        CancellationToken cancelToken = default) where T : JsonTableEntity, new()
    {
        var attr = typeof(T).GetCustomAttribute<TableKeyAttribute>();
        if (attr == null)
            throw new InvalidOperationException("This operation requires the entity to be decorated with a TableKeyAttribute.");

        var partitionKey = attr.PartitionKeyFormat;
        if (parameters != null)
            partitionKey = Smart.Format(partitionKey, parameters);

        var rowKey = attr.RowKeyFormat;
        if (parameters != null)
            rowKey = Smart.Format(rowKey, parameters);

        return await GetEntityAsync<T>(client, partitionKey, rowKey, select, cancelToken);
    }

    public static async Task<T> GetEntityAsync<T>(
        this TableClient client,
        string partitionKey,
        string rowKey,
        IEnumerable<string>? select = null,
        CancellationToken cancelToken = default) where T : JsonTableEntity, new()
    {
        var entity = await client.GetEntityAsync<TableEntity>(partitionKey, rowKey, select, cancelToken);
        if (entity == null || !entity.HasValue || entity.Value == null)
            // TODO: Better Error
            throw new InvalidOperationException("Cannot find entity.");

        T instance = new();
        instance.SetValue(entity.Value);

        return instance;
    }

    public static async Task BulkAddAsync<T>(this TableClient client, IEnumerable<T> items, int chunkSize = 25, CancellationToken cancelToken = default) where T : JsonTableEntity
        => await BulkTransactionAsync(client, TableTransactionActionType.Add, items, chunkSize, cancelToken);

    public static async Task BulkUpsertReplaceAsync<T>(this TableClient client, IEnumerable<T> items, int chunkSize = 25, CancellationToken cancelToken = default) where T : JsonTableEntity
        => await BulkTransactionAsync(client, TableTransactionActionType.UpsertReplace, items, chunkSize, cancelToken);

    public static async Task BulkUpsertMergeAsync<T>(this TableClient client, IEnumerable<T> items, int chunkSize = 25, CancellationToken cancelToken = default) where T : JsonTableEntity
        => await BulkTransactionAsync(client, TableTransactionActionType.UpsertMerge, items, chunkSize, cancelToken);

    private static async Task BulkTransactionAsync<T>(this TableClient client, TableTransactionActionType actionType, IEnumerable<T> items, int chunkSize = 25, CancellationToken cancelToken = default)
        where T : JsonTableEntity
    {
        if (chunkSize < 1)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be at least 1.");
        if (chunkSize > 100)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chuck size can be no larger than 100.");

        var entities = items.Select(i => i.GetValue());
        var partitionedGroups = entities.GroupBy(e => e.PartitionKey);
        foreach (var group in partitionedGroups)
        {
            var transactions = group.Select(e => new TableTransactionAction(actionType, e));
            var batches = transactions.Chunk(chunkSize);

            foreach (var batch in batches)
            {
                await client.SubmitTransactionAsync(batch, cancelToken);
                if (cancelToken.IsCancellationRequested)
                    return;
            }
        }
    }
}
