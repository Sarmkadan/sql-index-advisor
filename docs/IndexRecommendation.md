# IndexRecommendation

The `IndexRecommendation` class represents a structured suggestion for creating a new database index to improve query performance. It encapsulates the target table, the specific columns required for the index key and included columns, an estimated performance impact percentage, a confidence level, and the rationale behind the suggestion. Additionally, it provides a generated name for the index and the complete `CREATE INDEX` SQL statement ready for execution.

## API

### `Table`
```csharp
public required string Table { get; init; }
```
Gets the name of the database table for which the index is recommended. As a `required` member, this property must be initialized during object creation; failure to do so results in a compile-time error or a runtime `NullReferenceException` if bypassed via reflection.

### `KeyColumns`
```csharp
public required List<string> KeyColumns { get; init; }
```
Gets the ordered list of column names that constitute the key of the recommended index. The order of items in this list is significant as it determines the sort order and selectivity of the index. This property is `required` and must contain at least one column to form a valid index.

### `IncludeColumns`
```csharp
public List<string> IncludeColumns { get; init; }
```
Gets the list of non-key columns to be included in the index leaf level (covering columns). These columns allow queries to be satisfied entirely from the index without looking up the base table. This list may be empty if no covering columns are suggested.

### `EstimatedImpactPercent`
```csharp
public double EstimatedImpactPercent { get; init; }
```
Gets the estimated percentage improvement in query performance (e.g., reduction in execution time or I/O cost) if this index is created. The value is a floating-point number where higher values indicate greater potential benefit.

### `Confidence`
```csharp
public Confidence Confidence { get; init; }
```
Gets the confidence level associated with this recommendation. The `Confidence` enum indicates the reliability of the analysis based on the available statistics and execution plan data (e.g., High, Medium, Low).

### `Reasons`
```csharp
public List<string> Reasons { get; init; }
```
Gets a list of human-readable strings explaining the rationale behind this recommendation. Common reasons include "Missing index on join predicate," "High cost sort operation," or "Covering index eliminates key lookup."

### `SuggestedName`
```csharp
public string SuggestedName { get; init; }
```
Gets the automatically generated name for the new index, following standard naming conventions (e.g., `IX_TableName_Column1_Column2`). This value may be null if name generation failed or was not performed.

### `ToCreateStatement`
```csharp
public string ToCreateStatement { get; init; }
```
Gets the complete T-SQL `CREATE INDEX` statement required to implement this recommendation. This string includes the index name, target table, key columns, and any included columns. It is ready for direct execution against the database, barring permission or naming conflicts.

## Usage

### Example 1: Inspecting and Filtering Recommendations
This example demonstrates iterating through a list of recommendations, filtering for high-impact suggestions, and displaying their details.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public void ProcessRecommendations(List<IndexRecommendation> recommendations)
{
    var highImpact = recommendations
        .Where(r => r.EstimatedImpactPercent > 20.0 && r.Confidence == Confidence.High)
        .ToList();

    foreach (var rec in highImpact)
    {
        Console.WriteLine($"Table: {rec.Table}");
        Console.WriteLine($"Suggested Name: {rec.SuggestedName}");
        Console.WriteLine($"Impact: {rec.EstimatedImpactPercent:F2}%");
        Console.WriteLine("Key Columns: " + string.Join(", ", rec.KeyColumns));
        
        if (rec.IncludeColumns.Any())
        {
            Console.WriteLine("Include Columns: " + string.Join(", ", rec.IncludeColumns));
        }

        Console.WriteLine("Reasons:");
        foreach (var reason in rec.Reasons)
        {
            Console.WriteLine($"  - {reason}");
        }
        Console.WriteLine(new string('-', 40));
    }
}
```

### Example 2: Generating Deployment Scripts
This example shows how to extract the SQL creation statements for all valid recommendations to generate a migration script.

```csharp
using System;
using System.Text;
using System.Collections.Generic;

public string GenerateMigrationScript(List<IndexRecommendation> recommendations)
{
    var scriptBuilder = new StringBuilder();
    scriptBuilder.AppendLine("-- Auto-generated Index Recommendations");
    scriptBuilder.AppendLine($"-- Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
    scriptBuilder.AppendLine();

    foreach (var rec in recommendations)
    {
        if (!string.IsNullOrEmpty(rec.ToCreateStatement))
        {
            scriptBuilder.AppendLine(rec.ToCreateStatement);
            scriptBuilder.AppendLine("GO");
            scriptBuilder.AppendLine();
        }
        else
        {
            scriptBuilder.AppendLine($"-- Skipped: No creation statement available for {rec.Table}");
        }
    }

    return scriptBuilder.ToString();
}
```

## Notes

*   **Initialization Requirements**: Because `Table` and `KeyColumns` are marked as `required`, instances of `IndexRecommendation` cannot be created using the default parameterless constructor without immediately setting these fields. Attempting to deserialize this type from JSON requires the payload to contain these fields, or the deserializer must support required member handling (available in .NET 7+).
*   **Mutability**: While the collection properties (`KeyColumns`, `IncludeColumns`, `Reasons`) are initialized via `init` accessors, the lists themselves are mutable after construction. Callers can add or remove items from these lists unless the exposing code wraps them in read-only collections.
*   **Thread Safety**: The `IndexRecommendation` class is not inherently thread-safe for write operations. While reading properties is generally safe once the object is fully constructed, modifying the contents of the `List<string>` properties from multiple threads simultaneously requires external synchronization.
*   **SQL Injection**: The `ToCreateStatement` property contains a pre-formatted SQL string. While the advisor logic presumably sanitizes inputs, developers should verify the generated statement before executing it in production environments, especially if table or column names originate from untrusted user input.
*   **Empty Collections**: `IncludeColumns` and `Reasons` may be empty lists. Code consuming these properties should check `Count` or use `Any()` before iteration to avoid logic errors, though iterating over an empty list is safe. `KeyColumns` should logically never be empty for a valid index, but defensive coding is advised.
