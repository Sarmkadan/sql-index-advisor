# Index Recommendation Rules

This document describes every `IIndexRule` implementation in `src/SqlIndexAdvisor.Core/Rules/`:
what each rule detects, how it decides, what it recommends, and example plans it flags. It is
grounded in the code; if this document and the code disagree, the code wins and this document
needs fixing. The engine that runs the rules is described in [recommendation-engine.md](recommendation-engine.md).

## Rule inventory

| Class | `Name` (written to `Rule`) | Dialect | Base type | Output |
| --- | --- | --- | --- | --- |
| `EngineHintRule` | `engine-hint` | SQL Server | `PlanNodeVisitorBase` (overrides `Evaluate`) | Index |
| `FullScanWithFilterRule` | `fullscanwithfilter` | SQL Server, Postgres | `PlanNodeVisitorBase` | Index |
| `KeyLookupRule` | `keylookup` | SQL Server | `PlanNodeVisitorBase` | Index (or widen an existing one) |
| `MissingJoinIndexRule` | `missingjoinindex` | SQL Server, Postgres | `PlanNodeVisitorBase` | Index |
| `ImplicitConversionRule` | `implicit-conversion` | SQL Server, Postgres | `PlanNodeVisitorBase` (overrides `Evaluate`) | `SchemaFix`, no index |
| `ExpensiveSortRule` | `expensivesort` | SQL Server, Postgres | `PlanNodeVisitorBase` | Index |

`Name` values are the ones the rules return from `IIndexRule.Name`. Four of them come from
`PlanNodeVisitorBase.Name`, which lowercases the class name and strips `rule`. The other two
(`engine-hint`, `implicit-conversion`) are set by an override that reads a `*RuleConstants` class.

## How rules are selected

Which rules run depends on how the engine is built:

- `new RecommendationEngine()` and `new RecommendationEngine(RecommendationOptions)` run
  `DefaultRules.All()`: `EngineHintRule`, `FullScanWithFilterRule` and `ImplicitConversionRule`.
  The CLI uses the first form.
- `DefaultRuleSet.Create(options)` returns all six rules, in this order: `EngineHintRule`,
  `FullScanWithFilterRule`, `KeyLookupRule`, `ImplicitConversionRule`, `MissingJoinIndexRule`,
  `ExpensiveSortRule`. Pass that set to `new RecommendationEngine(IEnumerable<IIndexRule>)` to run
  `KeyLookupRule`, `MissingJoinIndexRule` and `ExpensiveSortRule`.

Rules are independent and side-effect free. Overlap between their outputs (for example, an engine
hint and a scan rule both proposing the same index) is resolved by `RecommendationMerger`, not inside
the rules.

## Shared concepts

### Relative cost

Every plan-tree rule compares `PlanNode.RelativeCost` (0..1), never an absolute cost. SQL Server and
Postgres cost units are not comparable. Each rule has a minimum share of statement cost below which
it stays silent:

| Rule | Minimum `RelativeCost` |
| --- | --- |
| `FullScanWithFilterRule` | 0.10 |
| `KeyLookupRule` | 0.10 |
| `MissingJoinIndexRule` | 0.10 |
| `ExpensiveSortRule` | 0.15 |
| `EngineHintRule`, `ImplicitConversionRule` | none (not cost-gated) |

### Confidence bands

Cost-gated rules grade confidence by the same bands, with the `ExpensiveSortRule` bands noted:

| Rule | `High` | `Medium` | `Low` |
| --- | --- | --- | --- |
| `FullScanWithFilterRule`, `KeyLookupRule`, `MissingJoinIndexRule` | ≥ 0.60 | ≥ 0.30 | otherwise |
| `ExpensiveSortRule` | ≥ 0.50 | ≥ 0.15 | otherwise |
| `ImplicitConversionRule` (max cost of the table's nodes) | ≥ 0.30 | ≥ 0.15 | otherwise |

`EngineHintRule` is always `High`. The scan rules may then lower confidence by row-count evidence
(see below).

### Row-count thresholds (`ScanRowStats`)

`FullScanWithFilterRule` and `MissingJoinIndexRule` use `ScanRowStats` to resolve row counts and
apply `RecommendationOptions`:

- Runtime counts are preferred: if `ActualRowsRead > 0`, those are used with `IsActual = true`.
  Otherwise the estimate is used: `TableCardinality`, falling back to `EstimatedRowsRead`.
- `MatchFraction = RowsReturned / RowsRead`, clamped to 0..1. It is `null` when `RowsRead` is unknown.
- **Table-size floor**: `MinTableRows` (default **1,000**). A scan reading fewer rows is suppressed.
- **Selectivity limit**: `MaxSelectivity` (default **0.5**). A scan whose filter returns more than
  this share of rows is suppressed.
- **Marginal downgrade**: when the match fraction is above half the selectivity limit (above 0.25 by
  default), confidence drops one level (`High` → `Medium`, otherwise `Low`).
- When the plan has no row counts, neither threshold applies. The reason text says so.

The thresholds are set through `RecommendationOptions` and passed in with `DefaultRules.All(options)`
or `DefaultRuleSet.Create(options)`.

---

## EngineHintRule (`engine-hint`)

**Source:** `src/SqlIndexAdvisor.Core/Rules/EngineHintRule.cs`
**Dialect:** SQL Server only (the hints come from the showplan `MissingIndexes` element).

**Detection.** It does not visit plan nodes (`ShouldVisit` is always `false`). It overrides `Evaluate`
and iterates `plan.EngineMissingIndexes`, one entry per optimizer hint.

**Recommendation.**
- Key columns: the hint's `EqualityColumns`, then its `InequalityColumns` that are not already keys.
- Include columns: the hint's `IncludeColumns`, minus any key column.
- A hint with no key columns is skipped.
- `EstimatedImpactPercent` is the hint's own `ImpactPercent`.
- Confidence is always `High`, because the optimizer costed the index itself.
- Reason: `Optimizer reported a missing index with {impact}% estimated impact.`

**Example.** The optimizer reports a missing index on `dbo.Orders` with equality columns `Status`,
`CreatedAt`, `CustomerId`, and includes `Total`, `ShippingAddress`, at 85.3% impact. The rule
produces key `(Status, CreatedAt, CustomerId)`, include `(Total, ShippingAddress)`, `High`, 85.3%.
That is the shape of `SqlServer/01_key_lookup_with_filter.expected.json`.

---

## FullScanWithFilterRule (`fullscanwithfilter`)

**Source:** `src/SqlIndexAdvisor.Core/Rules/FullScanWithFilterRule.cs`
**Constants:** `FullScanWithFilterRuleConstants.cs`
**Dialect:** SQL Server and Postgres.

**Detection.** A node passes `ShouldVisit` when all of these hold:
- Its operator is one of `Seq Scan` (Postgres), `Table Scan`, `Clustered Index Scan`, or `Index Scan`.
  `Index Scan` only qualifies when the node has predicate columns.
- It has a non-empty `TableName`.
- It has at least one `PredicateColumns` entry (the filter columns).
- `RelativeCost ≥ 0.10`.
- `ScanRowStats` clears both thresholds (table-size floor and selectivity limit).

**Recommendation.**
- Key columns: `PredicateColumns`, in the order the plan lists them.
- Include columns: `OutputColumns` that are not predicate columns, so the index can cover the query.
- Confidence: the cost band above, then the marginal-selectivity downgrade.
- Impact: `round(cost × 100 × (0.4 + 0.6 × eliminated), 1)`, where `eliminated = 1 − MatchFraction`.
  With no row counts, a neutral 50% match is assumed (`eliminated = 0.5`).
- Reason 1: `{operator} on {table} carries a filter on ({columns}) and is ~{cost}% of statement cost.`
- Reason 2+: the row-count evidence from `ScanRowStats.Describe`, which is either the scan's row
  numbers or a note that row counts are unknown. A downgrade adds one more sentence.

**Example (Postgres, `samples/postgres_users_seqscan.json`).** A `Seq Scan` on `users` filters on
`"country"` and `"is_active"`, costs about 92% of the statement, and has no row counts. The CLI
returns:

```
table:     users
keyColumns:     "country", "is_active"
includeColumns: id, email, created_at
confidence: High, estimatedImpactPercent: 64.4
originatingRule: fullscanwithfilter
```

**Example (SQL Server, `samples/sqlserver_orders_scan.sqlplan`).** A `Clustered Index Scan` on
`dbo.Orders` filters on `(Status, CreatedAt)`. Its recommendation is merged with the engine hint for
the same index, so `originatingRule` shows `engine-hint`, and both reasons appear.

---

## KeyLookupRule (`keylookup`)

**Source:** `src/SqlIndexAdvisor.Core/Rules/KeyLookupRule.cs`
**Constants:** `KeyLookupRuleConstants.cs`
**Dialect:** SQL Server only.

**Detection.** A node passes `ShouldVisit` when all of these hold:
- Its operator contains `Key Lookup` or `RID Lookup`.
- It has a non-empty `TableName`.
- It has a parent node.
- `RelativeCost ≥ 0.10`.

The row-count thresholds are not applied to this rule.

**Finding the seek.** The rule looks for a sibling of the lookup (same parent, not the same node)
whose operator contains `Seek` and which has predicate columns. If there is no such sibling, the
lookup's parent is used as the seek. The equality columns come from the seek's `PredicateColumns`.
If those are empty, the rule emits nothing.

**Recommendation.**
- Key columns: the seek's equality columns.
- Include columns: the lookup's `OutputColumns` that are not equality columns.
- `ExistingIndexName`: the seek's `IndexName`, when known. This means the existing index should be
  widened with the INCLUDE columns rather than a new index created.
- Confidence: the cost band above (no row-count downgrade).
- Impact: `round(cost × 100, 1)`.
- Reason: `{operator} on {table} follows an index seek with equality on ({columns}) and then performs a lookup; a covering index could avoid the extra lookup.`

**Example (`tests/SqlIndexAdvisor.Tests/Fixtures/SqlServer/01_key_lookup_with_filter.sqlplan`).** A
`Key Lookup` on `dbo.Orders` sits under a Nested Loops join, next to an `Index Seek` on
`IX_Orders_CustomerId`. The lookup returns `Total` and `ShippingAddress`. The expected output is key
`(Status, CreatedAt)`, include `(Total, ShippingAddress)`, `High`, 80.0%, with `rule: key-lookup`.

---

## MissingJoinIndexRule (`missingjoinindex`)

**Source:** `src/SqlIndexAdvisor.Core/Rules/MissingJoinIndexRule.cs`
**Constants:** `MissingJoinIndexRuleConstants.cs`
**Dialect:** SQL Server and Postgres.

**Detection.** A node passes `ShouldVisit` when all of these hold:
- `IsScan` is true (its operator contains `Scan` and not `Index Only`).
- It has a non-empty `TableName` and at least one `PredicateColumns` entry.
- `RelativeCost ≥ 0.10`.
- Its parent is a join: operator contains `Nested Loops`, `Hash Match`, or `Merge Join`.
- `ScanRowStats` clears both thresholds.

**Recommendation.**
- Key columns: the scan's predicate columns (the join predicate).
- Include columns: the scan's output columns that are not predicate columns.
- Confidence: the cost band above, then the marginal-selectivity downgrade.
- Impact: `round(cost × 100, 1)`.
- Reason 1: `Scan on {table} with join predicate on ({columns}) is part of a {join} join and is ~{cost}% of statement cost. An index on the join predicate columns would eliminate the full scan.`
- Reason 2+: row-count evidence, as for `FullScanWithFilterRule`.

**Example (`Fixtures/SqlServer/03_hash_join_without_index.sqlplan`).** A `Hash Match` joins
`dbo.Orders` and `dbo.Customers`. The `Customers` input is a `Table Scan` that carries the predicate
`Country = 'DE'` and sits directly under the join. The expected output is key `(Country)`, with reason
`Scan on customers with join predicate on (country) is part of a Hash Join join and is ~50% of statement cost.`

**Overlap with `FullScanWithFilterRule`.** Both rules can flag the same scan. The difference is
the trigger: `MissingJoinIndexRule` needs a join parent, while `FullScanWithFilterRule` needs a
full-scan operator with a filter. The merger deduplicates them when they propose the same index.

---

## ImplicitConversionRule (`implicit-conversion`)

**Source:** `src/SqlIndexAdvisor.Core/Rules/ImplicitConversionRule.cs`
**Constants:** `ImplicitConversionRuleConstants.cs`
**Dialect:** SQL Server (`CONVERT_IMPLICIT`) and Postgres (`::` casts).

**Detection.** It overrides `Evaluate`, so `ShouldVisit` is never used.
1. Look for conversions in `plan.StatementText`:
   - SQL Server: text `CONVERT_IMPLICIT(...)`. The column is the second argument, with any table or
     alias prefix stripped.
   - Postgres: each `::` cast. The column is the identifier immediately before the `::`.
   - A name is kept only if it is a plain identifier (letters, digits, `_`), with no `@` and no spaces.
2. Group the plan's nodes by `TableName`. For each table, keep only the converted columns that also
   appear in that table's `PredicateColumns` (case-insensitive).
3. A table with no matching columns produces nothing.

If `StatementText` is empty, the rule emits nothing. This is why a Postgres plan from
`EXPLAIN (FORMAT JSON)` alone does not trigger it, even when the filter contains `::text`. The
JSON carries no statement text.

**Recommendation.**
- `Kind = SchemaFix`. This rule does not propose an index.
- Key columns: the matching converted columns. Include columns: none.
- Confidence: the largest `RelativeCost` among the table's nodes (`High` ≥ 0.30, `Medium` ≥ 0.15).
- Impact: `round(first node's cost × 100, 1)`. This is a rough estimate.
- Reason: `Query contains implicit conversion(s) on column(s): {columns}`.
- The fix is to align the column and parameter types so the optimizer can seek.

**Example (unit test `ImplicitConversionRuleTests`).** Statement text
`SELECT * FROM orders WHERE CONVERT_IMPLICIT(varchar(10), status) = 'completed'`, with a
`Clustered Index Scan` on `orders` that filters on `status` and costs 0.8. The rule returns key
`status`, `High`, `SchemaFix`.

**Postgres example (scenario).** If the statement text is `SELECT * FROM products WHERE
product_code::text = 'ABC123'` and the plan has a `Seq Scan` on `products` that filters on
`product_code`, the rule returns key `product_code`, `SchemaFix`. This case is not in the fixtures;
it follows from the code above.

**Known gap.** Only `CONVERT_IMPLICIT` text is matched on SQL Server. An explicit `CAST` in the
statement is not matched. `SqlServer/04_implicit_conversion.expected.json` expects a result for
`WHERE CAST(ProductCode AS VARCHAR(20)) = 'ABC123'`, but the rule code does not match `CAST`.
Its `.sqlplan` fixture also fails to parse through the CLI.

---

## ExpensiveSortRule (`expensivesort`)

**Source:** `src/SqlIndexAdvisor.Core/Rules/ExpensiveSortRule.cs`
**Dialect:** SQL Server and Postgres.

**Detection.** A node passes `ShouldVisit` when all of these hold:
- Its operator starts with `Sort` or `TopSort`, equals `TopN Sort`, or is `Stream Aggregate` with
  at least one output column.
- `RelativeCost ≥ 0.15`.

The row-count thresholds are not applied to this rule.

**Sort columns.**
- For `Sort`, `TopSort`, and `TopN Sort` nodes: the node's `OutputColumns`.
- For `Stream Aggregate`: the node's `PredicateColumns`, which often hold the grouping columns.
- Duplicates are removed (case-insensitive). If there are no sort columns, nothing is emitted.

**Table.** The node's `TableName` if set. Otherwise the single table among its descendants. When
the sort consumes several tables (for example, above a join), no recommendation is made. The rule
does not guess.

**Recommendation.**
- Key columns: the sort columns, in order.
- Include columns: the node's `OutputColumns` that are not keys.
- Confidence: `High` ≥ 0.50, `Medium` ≥ 0.15, otherwise `Low`.
- Impact: `round(cost × 100 × (0.3 + 0.7 × min(rowFactor, 5) / 5), 1)`, where
  `rowFactor = clamp(EstimatedRowsRead / max(EstimatedRows, 1), 1, 10)`. A sort that reads many
  rows relative to the rows it emits scores higher.
- Reason: `Sort operation ({operator}) on {table} is ~{cost}% of statement cost. Creating an index on ({columns}) can eliminate or reduce this expensive sort.`

**Example (`Fixtures/SqlServer/06_expensive_sort.sqlplan`).** `SELECT * FROM dbo.OrderItems ORDER BY
OrderId DESC, LineNumber DESC`. A `Sort` at about 100% of cost on `dbo.OrderItems`. The expected
output is key `(OrderId, LineNumber)`, include `(ItemId, ProductId, Quantity, Price)`, `High`,
67.9%, with `rule: expensive-sort`.

---

## Adding a new rule

Follow the conventions in the repo's `CLAUDE.md`:
- Derive from `PlanNodeVisitorBase` (implement `ShouldVisit` and `VisitCore`), or implement
  `IIndexRule` directly for whole-plan analysis.
- Compare `RelativeCost`, never absolute cost.
- Put magic strings in a sibling `*RuleConstants` class.
- Every `IndexRecommendation` must carry human-readable `Reasons`.
- Register the rule in `DefaultRuleSet.Create`. Add it to `DefaultRules.All` only if it should run by
  default for `new RecommendationEngine()`.
- Add fixtures under `tests/SqlIndexAdvisor.Tests/Fixtures/<Dialect>/`.
