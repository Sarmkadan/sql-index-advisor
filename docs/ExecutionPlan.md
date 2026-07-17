# ExecutionPlan

The `ExecutionPlan` class represents a parsed SQL execution plan, typically obtained from a database query optimizer. It encapsulates both the overall plan characteristics—such as the dialect, statement text, and estimated cost—and a hierarchical tree of `PlanNode` objects that detail each operation. Additionally, it holds any missing index recommendations that the database engine identified during plan generation. This type is central to the `sql-index-advisor` project for analyzing index usage and suggesting improvements.

## API

### `public PlanDialect Dialect`
Gets the database dialect (e.g., SQL Server, PostgreSQL) for which the plan was generated.  
**Returns:** A `PlanDialect` enum value.  
**Throws:** Never.

### `public string StatementText`
Gets the original SQL statement text that produced the plan.  
**Returns:** A `string` containing the SQL.  
**Throws:** Never.

### `public double EstimatedTotalCost`
Gets the total estimated cost of the entire execution plan, as reported by the query optimizer.  
**Returns:** A `double` representing the cost (units vary by dialect).  
**Throws:** Never.

### `public List<PlanNode> Nodes`
Gets the list of top-level plan nodes. Each node represents an operation in the plan tree.  
**Returns:** A `List<PlanNode>` (may be empty if the plan has no nodes).  
**Throws:** Never.

### `public List<EngineMissingIndex> EngineMissingIndexes`
Gets the list of missing indexes that the database engine recommended for this query.  
**Returns:** A `List<EngineMissingIndex>` (may be empty).  
**Throws:** Never.

### `public string Operator`
Gets the operator name of the current plan node (e.g., "Index Scan", "Hash Join").  
**Returns:** A `string`.  
**Throws:** Never.

### `public string? TableName`
Gets the name of the table involved in this node, if applicable.  
**Returns:** A `string` or `null` if the node does not reference a table.  
**Throws:** Never.

### `public string? IndexName`
Gets the name of the index used by this node, if any.  
**Returns:** A `string` or `null` if no index is used.  
**Throws:** Never.

### `public double EstimatedRows`
Gets the estimated number of rows output by this node.  
**Returns:** A `double`.  
**Throws:** Never.

### `public double EstimatedRowsRead`
Gets the estimated number of rows read by this node (may differ from output rows due to filtering).  
**Returns:** A `double`.  
**Throws:** Never.

### `public double RelativeCost`
Gets the relative cost of this node as a fraction of the total plan cost (typically between 0 and 1).  
**Returns:** A `double`.  
**Throws:** Never.

### `public List<string> PredicateColumns`
Gets the list of columns used in predicates (WHERE, JOIN conditions) for this node.  
**Returns:** A `List<string>` (may be empty).  
**Throws:** Never.

### `public List<string> OutputColumns`
Gets the list of columns output by this node.  
**Returns:** A `List<string>` (may be empty).  
**Throws:** Never.

### `public PlanNode? Parent`
Gets the parent node in the plan tree, or `null` if this is the root node.  
**Returns:** A `PlanNode` or `null`.  
**Throws:** Never.

### `public string Table`
Gets the table name associated with this node. Unlike `TableName`, this property is always set (non-nullable) and may be empty if not applicable.  
**Returns:** A `string` (may be empty).  
**Throws:** Never.

### `public double ImpactPercent`
Gets the estimated performance impact percentage if a recommended missing index were created (typically 0–100).  
**Returns:** A `double`.  
**Throws:** Never.

### `public List<string> EqualityColumns`
Gets the list of columns used in equality predicates for a missing index recommendation.  
**Returns:** A `List<string>` (may be empty).  
**Throws:** Never.

### `public List<string> InequalityColumns`
Gets the list of columns used in inequality predicates (e.g., `>`, `<`) for a missing index recommendation.  
**Returns:** A `List<string>` (may be empty).  
**Throws:** Never.

### `public List<string> IncludeColumns`
Gets the list of columns that should be included as non-key columns in a missing index recommendation.  
**Returns:** A `List<string>` (may be empty).  
**Throws:** Never.

## Usage

### Example 1: Inspecting overall plan and iterating nodes

```csharp
using SqlIndexAdvisor;

// Assume 'plan' is an ExecutionPlan obtained from a parser
ExecutionPlan plan = GetPlan();

Console.WriteLine($"Dialect: {plan.Dialect}");
Console.WriteLine($"Statement: {plan.StatementText}");
Console.WriteLine($"Estimated Total Cost: {plan.EstimatedTotalCost}");

foreach (var node in plan.Nodes)
{
    Console.WriteLine($"Operator: {node.Operator}, Table: {node.Table}, Rows: {node.EstimatedRows}");
    if (node.Parent != null)
    {
        Console.WriteLine($"  Parent: {node.Parent.Operator}");
    }
}
```

### Example 2: Analyzing missing index recommendations

```csharp
using SqlIndexAdvisor;

ExecutionPlan plan = GetPlan();

if (plan.EngineMissingIndexes.Count > 0)
{
    Console.WriteLine("Missing indexes recommended:");
    foreach (var missingIndex in plan.EngineMissingIndexes)
    {
        Console.WriteLine($"  Table: {missingIndex.Table}");
        Console.WriteLine($"  Impact: {missingIndex.ImpactPercent}%");
        Console.WriteLine($"  Equality columns: {string.Join(", ", missingIndex.EqualityColumns)}");
        Console.WriteLine($"  Inequality columns: {string.Join(", ", missingIndex.InequalityColumns)}");
        Console.WriteLine($"  Include columns: {string.Join(", ", missingIndex.IncludeColumns)}");
    }
}
else
{
    Console.WriteLine("No missing indexes found.");
}
```

## Notes

- **Nullable properties:** `TableName`, `IndexName`, and `Parent` may be `null`. Always check for `null` before accessing their members.
- **Empty collections:** `Nodes`, `EngineMissingIndexes`, `PredicateColumns`, `OutputColumns`, `EqualityColumns`, `InequalityColumns`, and `IncludeColumns` can be empty lists. Iterating over them is safe.
- **Circular references:** The `Parent` property creates a tree structure. While cycles are not expected in valid plans, code that traverses the tree should guard against potential infinite loops (e.g., by tracking visited nodes).
- **Thread safety:** `ExecutionPlan` and its nested `PlanNode` objects are not thread-safe. If an instance is accessed concurrently from multiple threads, external synchronization is required. Mutating the lists (e.g., adding/removing nodes) after construction may lead to inconsistent state.
- **Cost values:** `EstimatedTotalCost`, `EstimatedRows`, `EstimatedRowsRead`, `RelativeCost`, and `ImpactPercent` are estimates provided by the query optimizer. They may be zero or negative in edge cases (e.g., trivial plans or unsupported dialects).
