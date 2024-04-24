# Sparcpoint.Extensions.Azure

[TODO]

## Installation

### Package Manager Console
```
Install-Package Sparcpoint.Extensions.Azure
```

### .NET Core CLI
```
dotnet add package Sparcpoint.Extensions.Azure
```

## Usage

### Table Storage

The table storage extensions allow for easy saving and loading from table storage using the `JsonTableEntity`. `JsonTableEntity` itself is NOT an `ITableEntity` but if used with the appropriate extensions included, allows for quick upserting. Additional features are included as well, utilizing attributes to define `PartitionKey` / `RowKey` formats.

First, you must inherit from `JsonTableEntity` and add whatever properties you would like to the entity. All basic types will use native table storage types while complex types will be serialized into JSON and stored in a string type.

Optionally, you can add a `TableKeyAttribute` to specify how the `PartitionKey` and `RowKey` are formed. These use properties names to form the resulting keys. Otherwise, you must specify `PartitionKey` and `RowKey` on the object manually.

```csharp
[TableKey(":subscriptions", "{SubscriptionName}")]
public class SubscriptionEntity : JsonTableEntity
{
  public string SubscriptionName { get; set; }
  public PaymentDetails PaymentMethod { get; set; }
  public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
}

public class PaymentDetails
{
  public string Name { get; set; }
  public string CardNumber { get; set; }
  public int CardExpirationYear { get; set; }
  public int CardExpirationMonth { get; set; }
  public string SecurityCode { get; set; }
}
```

In order to save this to a table you need a `TableClient`. Then use the `UpsertEntityAsync<T>` extension provided.

```csharp
public void Main(string[] args)
{
  string connectionString = "<your connection string>";
  string tableName = "<your table name>";

  var client = new TableClient(connectionString, tableName);
  var response = await client.UpsertEntityAsync(new SubscriptionEntity 
  {
    SubscriptionName = "Default",
    PaymentMethod = new PaymentDetails
    {
      Name = "John Smith",
      CardNumber = "1111000011110000",
      CardExpirationYear = 2025,
      CardExpirationMonth = 12,
      SecurityCode = 100
    }
  });
}
```

And that is all that is needed to insert / update your entity. The result will be a table with the following values:

| Column | Value |
| ------ | ----- |
| PartitionKey | `":subscriptions"` |
| RowKey | `"Default"` |
| SubscriptionName | `"Default"` |
| PaymentMethod | `"{"name":"John Smith","cardNumber":"1111000011110000","cardExpirationYear":2025,"cardExpirationMonth":12,"securityCode":100}"` |
| CreatedTimestamp | `2024-04-23T23:10:03Z` |
| Timestamp | `2024-04-23T23:10:03Z` |
| ETag | *[Generated Value]* |

#### Extension Methods

**Insert / Update an Entity**
`Task<Azure.Response> UpsertEntityAsync<T>(this TableClient client, T entity, TableUpdateMode updateMode = TableUpdateMode.Merge, CancellationToken cancelToken = default) where T : JsonTableEntity`

**Update an Entity Only (must exist)**
`Task<Azure.Response> UpdateEntityAsync<T>(this TableClient client, T entity, ETag etag, TableUpdateMode updateMode = TableUpdateMode.Merge, CancellationToken cancelToken = default) where T : JsonTableEntity`

**Insert an Entity Only (cannot exist)**
`Task<Azure.Response> AddEntityAsync<T>(this TableClient client, T entity, CancellationToken cancelToken = default) where T : JsonTableEntity`

**Query using a standard table query filter**
[MSDN / Storage Services / Querying tables and entities](https://learn.microsoft.com/en-us/rest/api/storageservices/querying-tables-and-entities)

`IAsyncEnumerable<T> QueryAsync<T>(this TableClient client, string filter, int? maxPerPage = null, IEnumerable<string>? select = null, [EnumeratorCancellation] CancellationToken cancelToken = default) where T : JsonTableEntity, new()`

**Perform a partition query based on `TableKeyAttribute`**
The `parameters` object is used to format the `PartitionKey` property of the `TableKeyAttribute` of `T`. So if the partition key format is `:groups:{GroupName}`, then the `parameters` object should have a `GroupName` property included.

`IAsyncEnumerable<T> PartitionQueryAsync<T>(this TableClient client, object? parameters = null, int? maxPerPage = null, IEnumerable<string>? select = null, CancellationToken cancelToken = default) where T : JsonTableEntity, new()`

**Perform a partition query based on a `string` value**
`IAsyncEnumerable<T> PartitionQueryAsync<T>(this TableClient client, string partitionKey, int? maxPerPage = null, IEnumerable<string>? select = null, CancellationToken cancelToken = default) where T : JsonTableEntity, new()`

**Retrieve an entity, if it exists, based on `TableKeyAttribute`**
The `parameters` object is used to format the `PartitionKey` property and the `RowKey` property of the `TableKeyAttribute` of `T`. So if the partition key format is `:subscriptions:{SubscriptionName}` and the row key format is `:users:{UserName}` then the `parameters` object should have both a `SubscriptionName` property and `UserName` property.

`Task<T?> GetEntityIfExistsAsync<T>(this TableClient client, object? parameters = null, IEnumerable<string>? select = null, CancellationToken cancelToken = default) where T : JsonTableEntity, new()`

**Retrieve an entity, if it exists, based on `string` values**
`Task<T?> GetEntityIfExistsAsync<T>(this TableClient client, string partitionKey, string rowKey, IEnumerable<string>? select = null, CancellationToken cancelToken = default) where T : JsonTableEntity, new()`

**Retrieve an entity, based on `TableKeyAttribute`**
The `parameters` object is used to format the `PartitionKey` property and the `RowKey` property of the `TableKeyAttribute` of `T`. So if the partition key format is `:subscriptions:{SubscriptionName}` and the row key format is `:users:{UserName}` then the `parameters` object should have both a `SubscriptionName` property and `UserName` property.

`Task<T> GetEntityAsync<T>(this TableClient client, object? parameters = null, IEnumerable<string>? select = null, CancellationToken cancelToken = default) where T : JsonTableEntity, new()`

**Retrieve an entity, based on `string` values**
`Task<T> GetEntityAsync<T>(this TableClient client, string partitionKey, string rowKey, IEnumerable<string>? select = null, CancellationToken cancelToken = default) where T : JsonTableEntity, new()`

**Bulk add entities**
`Task BulkAddAsync<T>(this TableClient client, IEnumerable<T> items, int chunkSize = 25, CancellationToken cancelToken = default) where T : JsonTableEntity`

**Bulk Insert / Update Entities (Replace)**
`Task BulkUpsertReplaceAsync<T>(this TableClient client, IEnumerable<T> items, int chunkSize = 25, CancellationToken cancelToken = default) where T : JsonTableEntity`

**Bulk Insert / Update Entities (Merge Properties)**
`Task BulkUpsertMergeAsync<T>(this TableClient client, IEnumerable<T> items, int chunkSize = 25, CancellationToken cancelToken = default) where T : JsonTableEntity`

## Examples

[TODO]