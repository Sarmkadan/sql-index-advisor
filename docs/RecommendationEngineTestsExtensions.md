# RecommendationEngineTestsExtensions

This static class provides extension methods and factory utilities for constructing and inspecting `IndexRecommendation` and `ExecutionPlan` instances within unit tests. It simplifies test setup by offering concise ways to create expected recommendations and plans, and to assert their properties without exposing internal constructors or comparers.

## API

### `HasKeyColumns`

```csharp
public static bool HasKeyColumns(this IndexRecommendation recommendation, params string[] expectedKeyColumns)
```

Checks whether the recommendation’s key columns match the specified set (order‑insensitive).  
**Parameters:**  
- `recommendation` – the recommendation to inspect.  
- `expectedKeyColumns` – one or more column names to compare.  

**Returns:** `true` if the key columns are exactly the same set as `expectedKeyColumns`; otherwise `false`.  

**Throws:**  
- `ArgumentNullException` if `recommendation` is `null`.  
- `ArgumentNullException` if any element of `expectedKeyColumns` is `null`.  

---

### `HasIncludeColumns`

```csharp
public static bool HasIncludeColumns(this IndexRecommendation recommendation, params string[] expectedIncludeColumns)
```

Checks whether the recommendation’s include columns match the specified set (order‑insensitive).  
**Parameters:**  
- `recommendation` – the recommendation to inspect.  
- `expectedIncludeColumns` – one or more column names to compare.  

**Returns:** `true` if the include columns are exactly the same set as `expectedIncludeColumns`; otherwise `false`.  

**Throws:**  
- `ArgumentNullException` if `recommendation` is `null`.  
- `ArgumentNullException` if any element of `expectedIncludeColumns` is `null`.  

---

### `HasConfidence`

```csharp
public static bool HasConfidence(this IndexRecommendation recommendation, double expectedConfidence)
```

Checks whether the recommendation’s confidence value equals the specified value (within double precision).  
**Parameters:**  
- `recommendation` – the recommendation to inspect.  
- `expectedConfidence` – the expected confidence value.  

**Returns:** `true` if `recommendation.Confidence` equals `expectedConfidence`; otherwise `false`.  

**Throws:**  
- `ArgumentNullException` if `recommendation` is `null`.  

---

### `CreateSeqScanPlan`

```csharp
public static ExecutionPlan CreateSeqScanPlan(string tableName)
```

Creates an `ExecutionPlan` representing a sequential scan of the given table.  
**Parameters:**  
- `tableName` – the name of the table being scanned.  

**Returns:** An `ExecutionPlan` instance configured for a sequential scan.  

**Throws:**  
- `ArgumentNullException` if `tableName` is `null`.  
- `ArgumentException` if `tableName` is empty or whitespace.  

---

### `CreateClusteredIndexScanPlan`

```csharp
public static ExecutionPlan CreateClusteredIndexScanPlan(string tableName, string indexName)
```

Creates an `ExecutionPlan` representing a clustered index scan on the specified table and index.  
**Parameters:**  
- `tableName` – the name of the table.  
- `indexName` – the name of the clustered index.  

**Returns:** An `ExecutionPlan` instance configured for a clustered index scan.  

**Throws:**  
- `ArgumentNullException` if `tableName` or `indexName` is `null`.  
- `ArgumentException` if either parameter is empty or whitespace.  

---

### `CreateIndexRecommendation`

```csharp
public static IndexRecommendation CreateIndexRecommendation(
    string indexName,
    string tableName,
    IReadOnlyList<string> keyColumns,
    IReadOnlyList<string> includeColumns,
    double confidence)
```

Creates a fully populated `IndexRecommendation` instance.  
**Parameters:**  
- `indexName` – the suggested index name.  
- `tableName` – the table on which the index is recommended.  
- `keyColumns` – the key columns of the index.  
- `includeColumns` – the included (non‑key) columns.  
- `confidence` – a confidence score (typically between 0.0 and 1.0).  

**Returns:** A new `IndexRecommendation` with the given properties.  

**Throws:**  
- `ArgumentNullException` if any string or collection argument is `null`.  
- `ArgumentException` if `indexName` or `tableName` is empty or whitespace.  
- `ArgumentException` if `keyColumns` is empty.  
- `ArgumentOutOfRangeException` if `confidence` is less than 0.0 or greater than 1.0.  

---

### `GetIndexName`

```csharp
public static string GetIndexName(this IndexRecommendation recommendation)
```

Returns the index name from the recommendation.  
**Parameters:**  
- `recommendation` – the recommendation to query.  

**Returns:** The index name as a string.  

**Throws:**  
- `ArgumentNullException` if `recommendation` is `null`.  

---

### `GetTableName`

```csharp
public static string GetTableName(this IndexRecommendation recommendation)
```

Returns the table name from the recommendation.  
**Parameters:**  
- `recommendation` – the recommendation to query.  

**Returns:** The table name as a string.  

**Throws:**  
- `ArgumentNullException` if `recommendation` is `null`.  

---

### `GetKeyColumns`

```csharp
public static IReadOnlyList<string> GetKeyColumns(this IndexRecommendation recommendation)
```

Returns the key columns from the recommendation.  
**Parameters:**  
- `recommendation` – the recommendation to query.  

**Returns:** An `IReadOnlyList<string>` containing the key column names.  

**Throws:**  
- `ArgumentNullException` if `recommendation` is `null`.  

---

### `GetIncludeColumns`

```csharp
public static IReadOnlyList<string> GetIncludeColumns(this IndexRecommendation recommendation)
```

Returns the include columns from the recommendation.  
**Parameters:**  
- `recommendation` – the recommendation to query.  

**Returns:** An `IReadOnlyList<string>` containing the include column names.  

**Throws:**  
- `ArgumentNullException` if `recommendation` is `null`.  

---

## Usage

### Example 1 – Creating a recommendation and asserting its properties

```csharp
[Fact]
public void CreateRecommendation_ShouldSetAllProperties()
{
    var keyColumns = new[] { "CustomerId", "OrderDate" };
    var includeColumns = new[] { "TotalAmount", "Status" };
    const double confidence = 0.85;

    var recommendation = RecommendationEngineTestsExtensions.CreateIndexRecommendation(
        "IX_Orders_CustomerId_OrderDate",
        "Orders",
        keyColumns,
        includeColumns,
        confidence);

    Assert.Equal("IX_Orders_CustomerId_OrderDate", recommendation.GetIndexName());
    Assert.Equal("Orders", recommendation.GetTableName());
    Assert.True(recommendation.HasKeyColumns("CustomerId", "OrderDate"));
    Assert.True(recommendation.HasIncludeColumns("TotalAmount", "Status"));
    Assert.True(recommendation.HasConfidence(0.85));
}
```

### Example 2 – Using execution plan factories in a test

```csharp
[Fact]
public void ShouldDetectSeqScanVsIndexScan()
{
    var seqPlan = RecommendationEngineTestsExtensions.CreateSeqScanPlan("Orders");
    var idxPlan = RecommendationEngineTestsExtensions.CreateClusteredIndexScanPlan("Orders", "PK_Orders");

    // Assume some method that compares plans
    Assert.NotEqual(seqPlan, idxPlan);
    Assert.Contains("Seq Scan", seqPlan.ToString());
    Assert.Contains("Clustered Index Scan", idxPlan.ToString());
}
```

---

## Notes

- **Edge cases:**  
  - Passing an empty `keyColumns` list to `CreateIndexRecommendation` throws `ArgumentException` because an index must have at least one key column.  
  - `HasKeyColumns` and `HasIncludeColumns` perform set‑based comparison; duplicate entries in either the recommendation or the expected array are ignored.  
  - Confidence values outside the [0.0, 1.0] range cause `ArgumentOutOfRangeException`.  
  - All string parameters are validated for null, empty, or whitespace; `ArgumentNullException` or `ArgumentException` is thrown accordingly.  

- **Thread‑safety:**  
  The static methods are thread‑safe because they do not access or modify any shared state. However, the `IndexRecommendation` and `ExecutionPlan` instances returned by these factories are not guaranteed to be thread‑safe. Tests should not concurrently mutate a single instance without external synchronization.
