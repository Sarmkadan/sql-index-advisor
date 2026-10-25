# Core model

This document describes the core model types in `src/SqlIndexAdvisor.Core/Model/` and how they relate. It covers the plan side (what the parsers produce and the rules consume), the recommendation side (what the rules and merger produce and the reporters render), and the two validation helpers that guard both.

> **Naming note.** The file names do not match every type name you might look for:
>
> | Name you may expect | Where it actually lives |
> | --- | --- |
> | `PlanModel` | `PlanModel.cs` declares `ExecutionPlan`, `PlanNode`, `EngineMissingIndex` and `PlanDialect`. There is no type called `PlanModel`. |
> | `Recommendation` | `Recommendation.cs` declares `IndexRecommendation`, `Confidence` and `RecommendationKind`. There is no type called `Recommendation`. |
> | `IIndexRecommendation` | `IIndexRecommendation.cs`. Implemented by `IndexRecommendation`. |
> | `ExecutionPlanValidation` / `IndexRecommendationValidation` | Static extension-method classes in `ExecutionPlanValidation.cs` and `IndexRecommendationValidation.cs`. |

## Overview

```
 parser (SqlServerXmlPlanParser / PostgresJsonPlanParser)
        │ produces
        ▼
 ExecutionPlan ──► PlanNode (flat list, Parent pointers)
        │      └──► EngineMissingIndex (engine-reported hints)
        │
        │  rules read ExecutionPlan, emit IndexRecommendation
        ▼
 IndexRecommendation : IIndexRecommendation
        │  uses PlanDialect, Confidence, RecommendationKind
        ▼
 Reporting renderers / DdlRenderer (take IndexRecommendation)

 ExecutionPlanValidation  ── validates ──► ExecutionPlan
 IndexRecommendationValidation ── validates ──► IndexRecommendation
```

## Plan types (`PlanModel.cs`)

### `PlanDialect` (enum)
```csharp
public enum PlanDialect { SqlServer, Postgres }
```
The database engine a plan came from. Rule thresholds differ slightly between the two. It is used by `ExecutionPlan.Dialect` and by `IndexRecommendation.ToCreateStatement(PlanDialect)`.

### `ExecutionPlan` (sealed class)
```csharp
public sealed class ExecutionPlan
{
    public PlanDialect Dialect { get; init; }
    public string StatementText { get; init; } = string.Empty;
    public double EstimatedTotalCost { get; init; }
    public List<PlanNode> Nodes { get; init; } = new();
    public List<EngineMissingIndex> EngineMissingIndexes { get; init; } = new();
}
```
The normalized form of a plan. Both the SQL Server XML shape and the Postgres JSON shape are flattened into this so the rules only handle one shape.

- `Dialect`: the source engine.
- `StatementText`: raw statement text if the plan carried it; may be empty.
- `EstimatedTotalCost`: the optimizer's estimated total cost. It is in engine-specific units and is reported, not compared. Rules use `PlanNode.RelativeCost` instead.
- `Nodes`: the operators, flattened (see `PlanNode`).
- `EngineMissingIndexes`: missing-index hints emitted by the engine itself (SQL Server does this). These are strong signals and are folded into recommendations.

Produced by `IPlanParser` implementations in `Parsing/`, such as `SqlServerXmlPlanParser` and `PostgresJsonPlanParser`. Consumed by the rules in `Rules/`.

### `PlanNode` (sealed class)
```csharp
public sealed class PlanNode
{
    public string Operator { get; init; } = string.Empty;
    public string? TableName { get; init; }
    public string? IndexName { get; init; }
    public double EstimatedRows { get; init; }
    public double EstimatedRowsRead { get; init; }
    public double TableCardinality { get; init; }
    public double ActualRows { get; init; }
    public double ActualRowsRead { get; init; }
    public double RelativeCost { get; init; }
    public List<string> PredicateColumns { get; init; } = new();
    public List<string> OutputColumns { get; init; } = new();
    public PlanNode? Parent { get; set; }
    public bool IsScan { get; }
}
```
One operator in the plan tree. The tree is stored as a flat list with a parent pointer rather than a child list, because the rules walk upward from scans.

- `Operator`: operator type, e.g. `Index Scan` or `Hash Join`.
- `TableName`, `IndexName`: the table and index the operator touches, if any. These are plain strings, not references to other objects.
- `EstimatedRows`: rows the optimizer expects the node to emit.
- `EstimatedRowsRead`: rows read from the source before filtering. `0` when the plan does not expose it. Postgres estimates report only post-filter rows.
- `TableCardinality`: rows stored in the scanned table (SQL Server `TableCardinality`). `0` when not exposed.
- `ActualRows`, `ActualRowsRead`: runtime counts across all executions, including rows discarded by a filter for `ActualRowsRead`. `0` when there are no runtime statistics.
- `RelativeCost`: the fraction of statement cost attributed to this node, in `0..1`. This is the only cost value rules compare.
- `PredicateColumns`: columns referenced by a residual predicate, in the order seen.
- `OutputColumns`: columns the node outputs. These are candidates for `INCLUDE`.
- `Parent`: the parent node, or `null` for the root. Settable; `SqlServerXmlPlanParser` sets it while building the tree.
- `IsScan`: derived. True when the operator name contains `Scan` and not `Index Only`.

### `EngineMissingIndex` (sealed class)
```csharp
public sealed class EngineMissingIndex
{
    public string Table { get; init; } = string.Empty;
    public double ImpactPercent { get; init; }
    public List<string> EqualityColumns { get; init; } = new();
    public List<string> InequalityColumns { get; init; } = new();
    public List<string> IncludeColumns { get; init; } = new();
}
```
A missing-index suggestion reported by the engine's own optimizer. `ImpactPercent` is expected in `0..100`. Validated by `ExecutionPlanValidation`.

## Recommendation types (`Recommendation.cs`, `IIndexRecommendation.cs`)

### `Confidence` (enum)
`Low`, `Medium`, `High`. The reliability of the recommendation, given the statistics and plan data available.

### `RecommendationKind` (enum)
- `CreateIndex`: create a new index. This is the default.
- `SchemaFix`: fix the query or schema instead, e.g. an implicit conversion or a parameter type mismatch.

### `IIndexRecommendation` (interface)
```csharp
public interface IIndexRecommendation
{
    string Table { get; init; }
    List<string> KeyColumns { get; init; }
    List<string> IncludeColumns { get; init; }
    double EstimatedImpactPercent { get; init; }
    double SourceNodeCost { get; init; }
    Confidence Confidence { get; init; }
    List<string> Reasons { get; init; }
    string SuggestedName(string? existingIndexName = null);
    string ToCreateStatement(PlanDialect dialect);
}
```
The read contract for an index suggestion. `IndexRecommendationSummaryExtensions` (e.g. `GetReasonsSummary`) is written against this interface. The reporters and `DdlRenderer` take the concrete `IndexRecommendation` class instead.

The interface does not declare `Kind`, `ExistingIndexName`, `Rule` or `ToString`. Those exist only on `IndexRecommendation`.

### `IndexRecommendation` (sealed class, implements `IIndexRecommendation`)
```csharp
public sealed class IndexRecommendation : IIndexRecommendation
{
    public required string Table { get; init; }
    public required List<string> KeyColumns { get; init; }
    public List<string> IncludeColumns { get; init; } = new();
    public double EstimatedImpactPercent { get; init; }   // 0..100
    public double SourceNodeCost { get; init; }           // 0..1
    public Confidence Confidence { get; init; }
    public string? Rule { get; set; }                     // set by the engine
    public List<string> Reasons { get; init; } = new();
    public RecommendationKind Kind { get; init; } = RecommendationKind.CreateIndex;
    public string? ExistingIndexName { get; init; }

    public string SuggestedName(string? existingIndexName = null);
    public string ToCreateStatement(PlanDialect dialect);
    public override string ToString();
}
```

- `Table`, `KeyColumns`: required. The table and the ordered index key.
- `IncludeColumns`: non-key columns for covering.
- `EstimatedImpactPercent`: a rough heuristic score, not a promise.
- `SourceNodeCost`: the `RelativeCost` of the plan node that prompted the suggestion.
- `Rule`: the name of the rule that produced it. Set by the engine, not by the rule.
- `Reasons`: human-readable reasons. Every recommendation must carry them.
- `Kind`: `CreateIndex` or `SchemaFix`.
- `ExistingIndexName`: if set, an existing index to widen with `INCLUDE` columns rather than a new index.

Behaviour of the methods:

- `SuggestedName(existingIndexName)` returns, in order of precedence: the argument if given, then `ExistingIndexName` if set, otherwise a generated name `IX_{Table}_{Key1}_{Key2}...`. Non-alphanumeric characters are stripped, and the table name is reduced to its last dot-separated part.
- `ToCreateStatement(dialect)` builds `CREATE INDEX <name> ON <Table> (<keys>) [INCLUDE (<cols>)];` using `SuggestedName()`. The `dialect` argument is currently not read by the method body, so both dialects produce the same text.
- `ToString()` gives a one-line summary, e.g. `INDEX ON dbo.Orders(CustomerId) INCLUDE (Total) - Impact: 42%, Confidence: High`, or `SCHEMA FIX for <Table> - ...` for `SchemaFix`.

Produced by the rules in `Rules/` and by `RecommendationMerger` in `Engine/`. Rendered by the `Reporting/` renderers (`IReportRenderer` takes `IReadOnlyList<IndexRecommendation>`) and by `DdlRenderer`.

## Validation helpers

Both helpers are static classes that expose `this`-extension methods. They return problems as human-readable strings and never mutate the object.

| Method | Returns | Throws |
| --- | --- | --- |
| `Validate()` | `IReadOnlyList<string>` of problems, empty when valid | `ArgumentNullException` on null input |
| `IsValid()` | `bool` | `ArgumentNullException` on null input |
| `EnsureValid()` | `void` | `ArgumentNullException` on null input; `ArgumentException` listing all problems when invalid |

### `ExecutionPlanValidation` (extends `ExecutionPlan`)
Checks the plan and its nodes and missing-index hints. Messages and numeric limits are in `ExecutionPlanValidationConstants` (internal).

Checked:
- `Dialect` is a defined `PlanDialect` value.
- `StatementText` is not null.
- `EstimatedTotalCost` is not NaN, infinite or negative.
- `Nodes` is not null. Each node is not null, has a non-empty `Operator`, has non-negative finite `EstimatedRows` and `EstimatedRowsRead`, has a finite `RelativeCost` in `[0, 1]`, and has non-null, non-empty entries in `PredicateColumns` and `OutputColumns`.
- `EngineMissingIndexes` is not null. Each entry is not null, has a non-empty `Table`, has a finite `ImpactPercent` in `[0, 100]`, and has non-null, non-empty entries in its three column lists.

### `IndexRecommendationValidation` (extends `IndexRecommendation`)
Checks a recommendation. Messages and limits are in `IndexRecommendationValidationConstants` (internal).

Checked:
- `Table` is not blank.
- `KeyColumns` is not null, contains at least one column, and has no blank entries.
- `IncludeColumns`, if not null, has no blank entries.
- `EstimatedImpactPercent` is in `[0, 100]`.
- `Reasons`, if not null, has no blank entries.

Not checked: `Confidence`, `Kind`, `SourceNodeCost`, `Rule` and `ExistingIndexName`. `Reasons` may be empty, even though the architecture expects every recommendation to carry reasons.

`IndexRecommendationValidation` extends the concrete `IndexRecommendation` class, not `IIndexRecommendation`. To validate an `IIndexRecommendation` value, cast it to `IndexRecommendation` first.

## Relationships

| From | Relationship | To | Notes |
| --- | --- | --- | --- |
| `ExecutionPlan` | owns, 1 to many | `PlanNode` | `Nodes` list. |
| `ExecutionPlan` | owns, 0 to many | `EngineMissingIndex` | `EngineMissingIndexes` list. |
| `ExecutionPlan` | uses | `PlanDialect` | `Dialect` property. |
| `PlanNode` | self-reference, 0..1 | `PlanNode` | `Parent` pointer; the flat list is the tree. |
| `IndexRecommendation` | implements | `IIndexRecommendation` | Interface is the read contract. |
| `IndexRecommendation` | uses | `Confidence`, `RecommendationKind` | Properties. |
| `IndexRecommendation` | uses | `PlanDialect` | Parameter of `ToCreateStatement`. |
| `Reporting` renderers | read | `IndexRecommendation` | Take `IReadOnlyList<IndexRecommendation>`. |
| `ExecutionPlanValidation` | validates | `ExecutionPlan` | Extension methods. |
| `IndexRecommendationValidation` | validates | `IndexRecommendation` | Extension methods. |

Data flow in the pipeline:

1. A parser builds an `ExecutionPlan` and its `PlanNode` tree.
2. Rules walk the tree from scan nodes, compare `RelativeCost` and `EngineMissingIndexes`, and emit `IndexRecommendation` objects with `Rule` and `Reasons` filled in.
3. `RecommendationMerger` de-duplicates and ranks them.
4. Renderers and `DdlRenderer` read `IndexRecommendation` directly, including `ToCreateStatement`.

## Conventions

- Cost comparisons use `PlanNode.RelativeCost` only. `ExecutionPlan.EstimatedTotalCost` is reported, not compared across dialects.
- Magic strings and numeric limits live in the sibling `*Constants` classes, not inline.
- Core has no `Console` or `File` access, so none of these types do I/O.
