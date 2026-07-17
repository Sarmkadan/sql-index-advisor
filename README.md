# sql-index-advisor

A small command-line tool that reads a query **execution plan** - SQL Server
showplan XML or PostgreSQL `EXPLAIN (FORMAT JSON)` - and points out indexes that
are probably missing, together with a rough estimate of how much they might
help.

It is deliberately conservative. The impact numbers are heuristics, not
measured gains, and every recommendation is something you should sanity-check
against your own schema and workload before running it in production.

## Why

`sys.dm_db_missing_index_details` and the green "missing index" hint in SSMS
already exist, but they only cover SQL Server, they only surface what the
optimizer happened to flag, and there is nothing equivalent you can pipe a
Postgres plan through in CI. This tool takes a saved plan file (or stdin),
normalizes both dialects into one shape, and runs a handful of rules over it so
you can wire it into a pull-request check or just eyeball a slow query.

## Install / build

Requires the .NET 10 SDK.

```bash
dotnet build
dotnet test
```

## Usage

```bash
# analyze a saved SQL Server plan
dotnet run --project src/SqlIndexAdvisor.Cli -- samples/sqlserver_orders_scan.sqlplan

# analyze a Postgres plan, emit JSON (handy for CI / jq)
dotnet run --project src/SqlIndexAdvisor.Cli -- samples/postgres_users_seqscan.json --format json

# pipe a plan in on stdin
psql -qAt -c "EXPLAIN (FORMAT JSON) SELECT ..." mydb \
  | dotnet run --project src/SqlIndexAdvisor.Cli -- --stdin

# only show recommendations worth at least 25% estimated impact
dotnet run --project src/SqlIndexAdvisor.Cli -- plan.sqlplan --min-impact 25
```

Options:

| Option | Meaning |
| --- | --- |
| `<plan-file>` | Path to the plan file. Format is auto-detected. |
| `--stdin` | Read the plan from standard input instead. |
| `--format text\|json` | Output format (default `text`). |
| `--min-impact <n>` | Hide recommendations below this estimated impact percent. |

Exit codes: `0` success, `1` usage/IO error, `2` the input could not be parsed
as a supported plan format.

## Example output

```
Dialect      : SqlServer
Statement cost: 4.812
Operators     : 1

1 recommendation(s):

[1] High confidence  |  ~82.5% estimated impact
    CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (CustomerId, Total);
      - Optimizer reported a missing index with 82.5% estimated impact.
      - Clustered Index Scan on dbo.Orders carries a filter on (Status, CreatedAt) and is ~100% of statement cost.

Impact figures are rough heuristics, not measured gains. Validate before applying.
```

## How it works

```
plan file ──► PlanParserFactory ──► ExecutionPlan (normalized) ──► RecommendationEngine ──► report
                 │                                                      │
   SqlServerXmlPlanParser                                        rules (IIndexRule):
   PostgresJsonPlanParser                                          - EngineHintRule
                                                                    - FullScanWithFilterRule
```

1. **Parsing.** Each parser flattens its dialect into a common `ExecutionPlan`
   (a list of `PlanNode`s plus any engine-emitted missing-index hints). The SQL
   Server parser matches elements by local name so it does not break when the
   showplan schema URI changes between versions. The Postgres parser pulls
   column names out of `Filter` / `Index Cond` expression strings with a light
   tokenizer (`PredicateColumnScanner`); predicates wrapped in a function such
   as `lower(name) = ...` are intentionally skipped because they are not
   sargable anyway.

2. **Rules.** Each rule implements `IIndexRule` and emits raw suggestions:
   - `EngineHintRule` trusts the optimizer's own missing-index output (SQL
     Server only). Highest confidence, because the engine costed it itself.
   - `FullScanWithFilterRule` flags a table / sequential scan that carries a
     filter and accounts for a meaningful share of the statement cost. Filter
     columns become the index key; other output columns become `INCLUDE`
     candidates so the index can cover the query.

3. **Merge & rank.** `RecommendationEngine` de-duplicates overlapping
   suggestions (same table, same leading key columns - the wider one wins and
   absorbs the other's includes and reasons), then orders by confidence and
   estimated impact.

## PlanParserFactoryExtensions

`PlanParserFactoryExtensions` provides a set of handy extension methods for
`PlanParserFactory`. They let you try parsing safely, batch‑parse many plans,
check whether content is parsable, enumerate registered parsers, and even
choose a parser manually with a custom selector.

**Example usage**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;

class Example
{
    static void Main()
    {
        var factory = new PlanParserFactory();

        // 1️⃣ Try to parse a single plan safely
        string content = File.ReadAllText("samples/sqlserver_orders_scan.sqlplan");
        if (factory.TryParse(content, out ExecutionPlan? plan))
        {
            Console.WriteLine($"Parsed plan with {plan.Nodes.Count} nodes.");
        }
        else
        {
            Console.WriteLine("Failed to parse the plan.");
        }

        // 2️⃣ Parse many plans from a collection of (sourceId, content) tuples
        var manyPlans = factory.ParseMany(new[]
        {
            ("plan1", content),
            ("plan2", File.ReadAllText("samples/postgres_users_seqscan.json"))
        });

        foreach (var (sourceId, execPlan) in manyPlans)
        {
            Console.WriteLine($"{sourceId}: {execPlan.Nodes.Count} nodes");
        }

        // 3️⃣ Quick check whether some content could be parsed at all
        bool canParse = factory.CanParse(content);
        Console.WriteLine($"Can parse content? {canParse}");

        // 4️⃣ List the names of all registered parsers
        IReadOnlyList<string> parserNames = factory.GetRegisteredParserNames();
        Console.WriteLine("Registered parsers: " + string.Join(", ", parserNames));

        // 5️⃣ Get the actual parser instances
        IReadOnlyList<IPlanParser> parsers = factory.GetRegisteredParsers();

        // 6️⃣ Parse using a custom selector (e.g., prefer the SQL Server parser)
        ExecutionPlan customPlan = factory.ParseWith(content, available =>
            available.FirstOrDefault(p => p is SqlServerXmlPlanParser));
        Console.WriteLine($"Custom parsed plan has {customPlan.Nodes.Count} nodes.");
    }
}
```

The snippet demonstrates the most common scenarios: safe single‑plan parsing,
batch parsing, capability checks, introspection of registered parsers, and
custom parser selection.


## ExecutionPlanExtensions

`ExecutionPlanExtensions` provides a set of extension methods for analyzing execution plans and identifying indexing opportunities. These methods help you programmatically inspect scan operations, predicate columns, output columns, and engine-provided missing index hints to build custom analysis or reporting tools.

**Example usage**

```csharp
using System;
using System.Linq;
using SqlIndexAdvisor.Core.Model;

class Example
{
    static void Main()
    {
        // Assume we have a parsed execution plan
        var factory = new PlanParserFactory();
        string sqlServerPlan = File.ReadAllText("samples/sqlserver_orders_scan.sqlplan");
        
        if (factory.TryParse(sqlServerPlan, out ExecutionPlan? plan))
        {
            // Analyze scan operations
            var scanCandidates = plan.GetScanCandidates();
            Console.WriteLine($"Found {scanCandidates.Count()} scan candidates");
            
            // Get total cost of all scans
            double totalScanCost = plan.GetTotalScanCost();
            Console.WriteLine($"Total scan cost: {totalScanCost:F3}");
            
            // Identify tables being scanned
            var scannedTables = plan.GetScannedTables().ToList();
            Console.WriteLine($"Scanned tables: {string.Join(", ", scannedTables)}");
            
            // Find columns used in predicates (good index key candidates)
            var predicateColumns = plan.GetPredicateColumns().ToList();
            Console.WriteLine($"Predicate columns: {string.Join(", ", predicateColumns)}");
            
            // Find columns that could be INCLUDE columns
            var includeCandidates = plan.GetIncludeCandidateColumns().ToList();
            Console.WriteLine($"Include candidate columns: {string.Join(", ", includeCandidates)}");
            
            // Check if there are indexable scans
            bool hasIndexableScans = plan.HasIndexableScans();
            Console.WriteLine($"Has indexable scans: {hasIndexableScans}");
            
            // Get the highest cost scan
            var highestCostScan = plan.GetHighestCostScan();
            if (highestCostScan != null)
            {
                Console.WriteLine($"Highest cost scan: {highestCostScan.TableName} ({highestCostScan.RelativeCost:P0})");
            }
            
            // Check engine missing index hints
            var equalityColumns = plan.GetMissingIndexEqualityColumns().ToList();
            var includeColumns = plan.GetMissingIndexIncludeColumns().ToList();
            Console.WriteLine($"Engine suggests {equalityColumns.Count} equality columns and {includeColumns.Count} include columns");
            
            // Find columns already covered by scans
            var coveredColumns = plan.GetAlreadyCoveredColumns().ToList();
            Console.WriteLine($"Columns already covered: {string.Join(", ", coveredColumns)}");
        }
    }
}
```

The example demonstrates how to use the extension methods to analyze execution plans programmatically, extract indexing opportunities, and work with both scan-based analysis and engine-provided missing index hints.



## Limits / not done yet

- No column-order awareness beyond equality-before-inequality. It will not
  reorder keys by selectivity because a plan alone does not carry that.
- It does not check whether a similar index already exists on the table - it
  only sees the single plan you give it.
- Postgres predicate parsing is string-based; unusual expression formatting can
  cause a column to be missed. When in doubt, read the reasons it prints.

## License

MIT.
