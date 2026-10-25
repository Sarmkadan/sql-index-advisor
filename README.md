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

## Configuration and typical scenarios

There is no configuration file. Behaviour is controlled by CLI options (parsed
by `ArgsParser` in `SqlIndexAdvisor.Core.ArgsParsing`) and, when you use the
library, by the rule set and the filters you apply to the returned list.

### Options reference

| Option | Type / default | Effect |
| --- | --- | --- |
| `<plan-file>` or `--plan <path>` | path, required unless `--stdin` | Plan to analyze. Mutually exclusive with `--stdin`. |
| `--stdin` (or `-`) | flag | Read the plan from standard input. |
| `--format text\|json\|html\|csv` | default `text` | Output format. The CLI does not accept `markdown` (see Limitations). |
| `--min-impact <n>` | number, default `0` (no filter) | Hide recommendations whose `EstimatedImpactPercent` is below `n`. |
| `--fail-on-findings` | flag | Exit `1` when at least one recommendation is printed. |
| `--fail-on low\|medium\|high` | severity, default off | Exit `1` when any recommendation's severity (its confidence) is at or above the given level. |
| `--sqlserver` / `--postgres` | flags | Accepted, mutually exclusive, currently ignored - the dialect is auto-detected. |
| `--version`, `-h` / `--help` | flags | Print version or usage. |

Exit codes: `0` success, `1` usage/IO error **or** findings present with
`--fail-on-findings` (so `1` is not always an error there - check the output),
`2` the input could not be parsed.

There is no option for a maximum number of suggestions, for excluding existing
indexes, or for analyzing a directory. The scenarios below show how to get
each of those from code or from the shell.

### Scenario: analyze a directory of plans

The CLI takes one plan per invocation. For a directory, loop in the shell and
write one report per plan. Plan paths must be relative to (or inside) the
current directory and must not contain `..`.

```bash
mkdir -p reports
for f in plans/*.sqlplan plans/*.json; do
  dotnet run --project src/SqlIndexAdvisor.Cli -- "$f" --format csv > "reports/$(basename "$f").csv" \
    || echo "failed: $f"
done
```

From code, `PlanParserFactory` and `RecommendationEngine` can be reused across
files. Plans that fail to parse throw `PlanParseException`, so catch it per file:

```csharp
using System;
using System.IO;
using System.Linq;
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;

var factory = new PlanParserFactory();
var engine = new RecommendationEngine();

foreach (string path in Directory.EnumerateFiles(args[0], "*.*")
    .Where(f => f.EndsWith(".sqlplan", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
    .OrderBy(f => f))
{
    try
    {
        // One file can hold several statements, so parse it into one plan per statement.
        var recCount = factory.Parse(File.ReadAllText(path)).Sum(plan => engine.Analyze(plan).Count);
        Console.WriteLine($"{Path.GetFileName(path)}: {recCount} recommendation(s)");
    }
    catch (PlanParseException ex)
    {
        Console.Error.WriteLine($"skipped {Path.GetFileName(path)}: {ex.Message}");
    }
}
```

### Scenario: exclude indexes that already exist

The engine only sees the plan, so it can suggest an index that is already in
place. There is no flag for this; filter the results yourself. A suggestion is
dropped when an existing index on the same table starts with the same key
columns. Quoting such as `"country"` or `[Status]` is stripped before comparing.

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;

// Indexes that already exist in the target database, one entry per index.
var existing = new List<(string Table, string[] Keys)>
{
    ("dbo.Orders", new[] { "Status", "CreatedAt", "Total" }),
    ("users", new[] { "country" }),
};

static string Normalize(string column) => column.Trim('"', '[', ']');

static bool AlreadyIndexed(IndexRecommendation rec, IEnumerable<(string Table, string[] Keys)> indexes) =>
    indexes.Any(ix =>
        string.Equals(ix.Table, rec.Table, StringComparison.OrdinalIgnoreCase) &&
        ix.Keys.Length >= rec.KeyColumns.Count &&
        rec.KeyColumns.Select(Normalize).SequenceEqual(ix.Keys.Take(rec.KeyColumns.Count), StringComparer.OrdinalIgnoreCase));

var recs = new PlanParserFactory().Parse(File.ReadAllText(args[0]))
    .SelectMany(plan => new RecommendationEngine().Analyze(plan))
    .Where(rec => !AlreadyIndexed(rec, existing))
    .ToList();

Console.WriteLine($"{recs.Count} new index suggestion(s) after excluding existing indexes");
```

Fill `existing` from your catalog (`sys.indexes` / `sys.index_columns` on SQL
Server, `pg_indexes` on Postgres). The list is hand-maintained; the tool does not
read it for you. Only leading key columns are compared, so an existing index on
`(Status, CreatedAt, Total)` covers a suggestion on `(Status, CreatedAt)`.

### Scenario: set a benefit threshold and a maximum suggestion count

From the CLI, `--min-impact` sets the threshold and there is no cap - use `jq`
to cap the JSON output:

```bash
dotnet run --project src/SqlIndexAdvisor.Cli -- plan.sqlplan --min-impact 25 --format json \
  | jq '.recommendations[:3]'
```

From code, the same two values are available as `ArgsParser.ParseResult`
properties (`MinImpact`) and as `IndexRecommendation.EstimatedImpactPercent`:

```csharp
using System;
using System.IO;
using System.Linq;
using SqlIndexAdvisor.Core.ArgsParsing;
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Parsing;

ArgsParser.ParseResult options = ArgsParser.Parse(new[] { "plan.sqlplan", "--min-impact", "25" });
if (!options.IsSuccess)
{
    Console.Error.WriteLine(options.ErrorMessage ?? options.HelpMessage);
    return 1;
}

const int maxSuggestions = 3;
// Each statement of the input is analyzed on its own. Rank the combined list by confidence, then impact. Filter, then cap.
var top = new PlanParserFactory().Parse(File.ReadAllText(args[0]))
    .SelectMany(plan => new RecommendationEngine().Analyze(plan))
    .OrderByDescending(rec => rec.Confidence)
    .ThenByDescending(rec => rec.EstimatedImpactPercent)
    .Where(rec => rec.EstimatedImpactPercent >= options.MinImpact)
    .Take(maxSuggestions)
    .ToList();

Console.WriteLine($"{top.Count} suggestion(s) at or above {options.MinImpact}% impact");
return 0;
```

Note that `Take` after the default ranking keeps the most confident suggestions,
not necessarily the highest-impact ones. To cap by impact, add
`.OrderByDescending(rec => rec.EstimatedImpactPercent)` before `Take`.

### Scenario: read the output columns

The Markdown report renders one table row per recommendation. Its columns map to:

| Column | Meaning |
| --- | --- |
| Table | Table the index would be created on. |
| Key columns | Index key, in order. Filter columns from the plan, in the order the rule produced them. |
| Includes | `INCLUDE` columns - non-key columns the query reads so the index can cover it. |
| Confidence | `High`, `Medium` or `Low`. Optimizer missing-index hints are always `High`. Scan-based rules grade by share of statement cost: `High` at 60% or more, `Medium` at 30% or more, otherwise `Low`. |
| Estimated impact % | Estimated share of statement cost the index could remove. Not a measured reduction. |
| Reason | Why the rule fired, joined with `; `. |

The CSV report uses the same data with the headers `Confidence,ImpactPercent,CreateStatement,Reasons`.
The text report shows the same numbers, with the `CREATE INDEX` statement and reasons indented under each entry.

```csharp
using System;
using System.IO;
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Parsing;
using SqlIndexAdvisor.Core.Reporting;

var plan = new PlanParserFactory().Parse(File.ReadAllText(args[0]));
var recs = new RecommendationEngine().Analyze(plan);

// Markdown table: | Table | Key columns | Includes | Confidence | Estimated impact % | Reason |
Console.WriteLine(MarkdownReportRenderer.RenderMarkdown(plan, recs));

// CSV: Confidence,ImpactPercent,CreateStatement,Reasons
Console.WriteLine(CsvReportRenderer.RenderCsv(plan, recs));
```

### Limitations

- **Input dialects.** Only SQL Server showplan XML and PostgreSQL
  `EXPLAIN (FORMAT JSON)` are supported. Other engines are rejected with exit `2`.
- **Encoding.** A UTF-16 `.sqlplan` without a byte-order mark is not detected.
  `samples/sqlserver_orders_scan_utf16le.sqlplan` fails for this reason; save the
  plan as UTF-8 or UTF-16 with a BOM.
- **Estimates, not measurements.** Impact is a heuristic share of the plan's
  estimated cost. Nothing is executed, timed, or checked against real data
  distributions, so an 80% estimate can be a 5% real gain (or no gain at all).
- **No view of existing indexes.** Each plan is analyzed alone. Use the exclusion
  scenario above, or validate against `sys.indexes` / `pg_indexes` yourself.
- **No write-side or storage cost.** Suggestions do not account for insert/update
  overhead, index size, or other queries that will slow down.
- **No workload aggregation.** Plans are not merged or weighted by frequency across
  a batch; ten plans with the same missing index produce ten independent results.
- **Postgres predicates are parsed as text.** See the notes in
  [Limits / not done yet](#limits--not-done-yet).
- **CLI format gap.** `--format markdown` is rejected by the CLI, although
  `MarkdownReportRenderer` exists and is used from code.
- **`--sqlserver` / `--postgres` do nothing** beyond the mutual-exclusion check.

## Getting started

To use the library from your own code instead of the CLI, reference
`SqlIndexAdvisor.Core` and follow the same three steps the CLI runs:

| Step | Type | Namespace |
| --- | --- | --- |
| Parse the plan into an `ExecutionPlan` | `PlanParserFactory` | `SqlIndexAdvisor.Core.Parsing` |
| Run the rules, merge and rank suggestions | `RecommendationEngine` | `SqlIndexAdvisor.Core.Engine` |
| Render `CREATE INDEX` DDL for a suggestion | `DdlRenderer` | `SqlIndexAdvisor.Core.Model` |

The impact number printed below is `IndexRecommendation.EstimatedImpactPercent`,
the same heuristic share of statement cost the CLI reports. It is not a
measured gain.

Create a console app outside this repository and reference the core project by
its path (replace `/path/to/sql-index-advisor` with your checkout):

```bash
dotnet new console -o GettingStarted
cd GettingStarted
dotnet add reference /path/to/sql-index-advisor/src/SqlIndexAdvisor.Core/SqlIndexAdvisor.Core.csproj
```

Replace the generated `Program.cs` with this:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;

if (args.Length != 1)
{
    Console.Error.WriteLine("usage: GettingStarted <plan-file>");
    return 1;
}

// 1. Read a SQL Server showplan XML or PostgreSQL EXPLAIN (FORMAT JSON) file.
string planPath = args[0];
string planText = File.ReadAllText(planPath);

// 2. Parse it into normalized ExecutionPlans, one per statement. The dialect is auto-detected.
var parserFactory = new PlanParserFactory();
IReadOnlyList<ExecutionPlan> plans = parserFactory.Parse(planText);
ExecutionPlan plan = plans[0]; // this sample analyzes the first statement only

// 3. Run the default rules, merge duplicates, and rank the suggestions.
var engine = new RecommendationEngine();
IReadOnlyList<IndexRecommendation> recommendations = engine.Analyze(plan);

// 4. Print CREATE INDEX DDL for each new-index suggestion. Schema fixes and
//    suggestions that widen an existing index (ExistingIndexName) are skipped,
//    because DdlRenderer would emit a CREATE with the existing index name.
var creates = recommendations
    .Where(r => r.Kind == RecommendationKind.CreateIndex && r.ExistingIndexName is null)
    .ToList();

Console.WriteLine($"{creates.Count} index recommendation(s) for {planPath} ({plan.Dialect}):");
foreach (var rec in creates)
{
    Console.WriteLine();
    Console.WriteLine(DdlRenderer.RenderCreateIndex(rec, plan.Dialect));
    Console.WriteLine($"-- estimated benefit: ~{rec.EstimatedImpactPercent:F1}% of statement cost ({rec.Confidence} confidence)");
}

return 0;
```

Run it against one of the sample plans:

```bash
dotnet run -- /path/to/sql-index-advisor/samples/sqlserver_orders_scan.sqlplan
dotnet run -- /path/to/sql-index-advisor/samples/postgres_users_seqscan.json
```

The SQL Server sample prints:

```
1 index recommendation(s) for /path/to/sql-index-advisor/samples/sqlserver_orders_scan.sqlplan (SqlServer):

CREATE NONCLUSTERED INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (OrderId, CustomerId, Total) WITH (ONLINE = ON);
-- estimated benefit: ~82.5% of statement cost (High confidence)
```

Notes on the generated DDL:

- SQL Server output uses `WITH (ONLINE = ON)`, which needs an edition that
  supports online index operations.
- Postgres output uses `CREATE INDEX CONCURRENTLY`, which cannot run inside a
  transaction block. Run it outside `BEGIN ... COMMIT`.

## Supported execution plan parsers

Two parsers implement `IPlanParser` in `SqlIndexAdvisor.Core.Parsing`:

| Parser | Input | Claims the content when | Result |
| --- | --- | --- | --- |
| `SqlServerXmlPlanParser` | SQL Server showplan XML (`.sqlplan`) | After leading whitespace the text starts with `<` and contains `ShowPlanXML` or `StmtSimple` (case-insensitive). | One `ExecutionPlan` per statement that carries a plan, in document order. |
| `PostgresJsonPlanParser` | PostgreSQL `EXPLAIN (FORMAT JSON)` output | After leading whitespace the text starts with `[` or `{` and contains a `"Node Type"` or `"Plan"` property name (case-insensitive). | One `ExecutionPlan` per top-level array entry; a bare top-level object is a single statement. |

The XML and JSON checks cannot both match the same content, because they
require different first characters.

### How `PlanParserFactory` selects a parser

`PlanParserFactory` (`SqlIndexAdvisor.Core.Parsing`) does not look at file
names or command-line flags. It sniffs the content:

1. It walks its registered parsers in order and asks each one `CanParse(content)`.
2. The first parser that returns `true` is used. Its `Parse` method produces the plans.
3. If no parser claims the content, `PlanParseException` is thrown. Its message lists the
   parsers that were tried. The CLI reports this as exit code `2`.

The parameterless constructor registers `SqlServerXmlPlanParser` first and
`PostgresJsonPlanParser` second. The CLI uses this default set. To use a
different set or order, pass your own parsers to the
`PlanParserFactory(IEnumerable<IPlanParser>)` constructor. This is how a new
dialect is added: implement `IPlanParser` and register it there.

Related members:

| Member | Behavior |
| --- | --- |
| `Parse(string)` | Resolves the parser and returns `IReadOnlyList<ExecutionPlan>`. Throws `PlanParseException` when nothing matches. |
| `Resolve(string)` | Returns the matching `IPlanParser`. Throws `PlanParseException` when nothing matches. |
| `TryResolve(string, out IPlanParser?)` | Returns `false` instead of throwing when nothing matches. |
| `RegisteredParserNames` | Type names of the registered parsers, in order. |

Edge cases:

- `null` input throws `ArgumentNullException`.
- Empty input (`""`) throws `ArgumentException` from the first parser's `CanParse`.
- Whitespace-only input matches no parser, so it throws `PlanParseException`.
- A parser can still throw `PlanParseException` from `Parse` after it has
  claimed the content, for example for malformed XML or JSON, a missing `Plan`
  object, or input over the size or nesting limits.

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

## Built-in rules

Every rule implements `IIndexRule` (`SqlIndexAdvisor.Core.Rules`). Rules only
emit raw `IndexRecommendation`s; de-duplication and ranking happen later in
`RecommendationMerger`. Each rule's `Name` is the value written to the
recommendation's `Rule` field.

Which rules run depends on how the engine is built:

- `new RecommendationEngine()` and `new RecommendationEngine(RecommendationOptions)`
  run `DefaultRules.All()`: `EngineHintRule`, `FullScanWithFilterRule` and
  `ImplicitConversionRule`.
- `DefaultRuleSet.Create(options)` returns all six rules listed below. It is
  not used by the engine's default constructors; pass its rules to
  `new RecommendationEngine(IEnumerable<IIndexRule>)` to run the other three
  (`KeyLookupRule`, `MissingJoinIndexRule`, `ExpensiveSortRule`).

The rules that scan the plan tree skip nodes whose `RelativeCost` is below
their minimum share of the statement cost. Cost is always compared as a
relative value (0..1), never as an absolute number, because SQL Server and
Postgres costs are not comparable.

### EngineHintRule (`engine-hint`)

- **Dialect:** SQL Server only.
- **Detects:** the missing-index hints the optimizer already put in the plan
  (`plan.EngineMissingIndexes`). It does not look at individual nodes.
- **Recommends:** the hinted table, with key columns taken from the hint's
  equality columns followed by its inequality columns (duplicates skipped),
  and the hint's include columns minus the key columns.
- **Confidence:** always `High`, because the engine costed the index itself.
  Hints with no key columns are skipped.
- **Impact:** the hint's own `ImpactPercent`.

### FullScanWithFilterRule (`fullscanwithfilter`)

- **Dialect:** SQL Server and Postgres.
- **Detects:** a full scan that carries a filter: `Table Scan`,
  `Clustered Index Scan`, `Index Scan` (only when it has predicate columns) or
  Postgres `Seq Scan`, on a named table with predicate columns, at least 10%
  of statement cost, and row counts that pass the `RecommendationOptions`
  thresholds (`MinTableRows`, `MaxSelectivity`).
- **Recommends:** the predicate columns as the key. Output columns that are not
  in the key become `INCLUDE` columns, so the index can cover the query.
- **Confidence:** graded by share of statement cost (`High` at 60% or more,
  `Medium` at 30% or more, otherwise `Low`), then adjusted by the row-count
  statistics.
- **Impact:** a blend of the scan's cost share and how much the filter is
  expected to eliminate. Without row counts a neutral 50% match is assumed.

### KeyLookupRule (`keylookup`)

- **Dialect:** SQL Server only.
- **Detects:** a `Key Lookup` or `RID Lookup` on a named table, with a parent
  node, at least 10% of statement cost. The lookup is usually the inner input of
  a Nested Loops join.
- **Finds the seek:** the sibling node under the same parent whose operator
  contains `Seek` and that has predicate columns. If there is none, the parent
  node is used.
- **Recommends:** the seek's equality columns as the key and the lookup's output
  columns that are not equality columns as `INCLUDE` columns, so the index
  covers the lookup. When the seek
  names an existing index, the recommendation records it in
  `ExistingIndexName`, meaning the existing index should be widened.
- **Confidence:** graded by share of statement cost (`High` at 60% or more,
  `Medium` at 30% or more, otherwise `Low`). Lookups with no equality columns
  on the seek are skipped.

### MissingJoinIndexRule (`missingjoinindex`)

- **Dialect:** SQL Server and Postgres.
- **Detects:** a scan with join predicate columns whose parent is a Nested
  Loops, Hash Match or Merge Join, on a named table, at least 10% of statement
  cost, and with row counts that pass the `RecommendationOptions` thresholds.
- **Recommends:** the scan's predicate columns as the key. Its other output
  columns become `INCLUDE` columns.
- **Confidence:** graded by share of statement cost (`High` at 60% or more,
  `Medium` at 30% or more, otherwise `Low`), then adjusted by row-count
  statistics.
- **Impact:** the scan's share of statement cost, as a percentage.

### ImplicitConversionRule (`implicit-conversion`)

- **Dialect:** SQL Server (`CONVERT_IMPLICIT`) and Postgres (`::` casts),
  read from the statement text.
- **Detects:** a conversion applied to a column in a predicate. It looks for
  `CONVERT_IMPLICIT(...)` in the statement text, or for a column cast with
  `::`, and keeps only names that look like column names. Only tables with
  nodes in the plan are considered.
- **Recommends:** no index. It sets `Kind = RecommendationKind.SchemaFix`
  and lists the affected columns in its reason. The fix is to align the column
  and parameter types, so an index seek is possible. The key columns are the
  affected columns, and there are no `INCLUDE` columns.
- **Confidence:** from the largest relative cost among the table's nodes
  (`High` at 0.30 or more, `Medium` at 0.15 or more, otherwise `Low`).
- **Impact:** a rough estimate based on the cost of the table's first plan
  node.

### ExpensiveSortRule (`expensivesort`)

- **Dialect:** SQL Server and Postgres.
- **Detects:** a sort operation at least 15% of statement cost. This covers
  operators starting with `Sort` or `TopSort`, `TopN Sort`, and
  `Stream Aggregate` when it has output columns. The sort's table is found from
  the node or from the plan.
- **Recommends:** the sort columns as the key, and the sort's output columns
  that are not in the key as `INCLUDE` columns. This lets an index return rows
  already in order.
- **Confidence:** `High` at 50% of statement cost or more, `Medium` at 15% or
  more, otherwise `Low`.
- **Impact:** the sort's share of cost, scaled by the ratio of rows read to
  rows produced (clamped to 1x-10x). A sort that processes many rows scores
  higher.

## RecommendationEngine

`RecommendationEngine` is the stage between parsing and reporting. It takes one
normalized `ExecutionPlan` (produced by `PlanParserFactory`), runs every
configured `IIndexRule` over it, merges the raw suggestions, and returns them
ranked. It does not read files, does not connect to a database, and does not
render output - the CLI and the report renderers consume its result.

```
ExecutionPlan ──► RecommendationEngine.Analyze ──► IReadOnlyList<IndexRecommendation> ──► DdlRenderer / ReportRenderer
                          │
                          ├─ run each IIndexRule           raw suggestions
                          ├─ RecommendationMerger.Merge    de-dup, prefix merge, implicit-conversion filter
                          └─ rank                          Confidence (desc), then EstimatedImpactPercent (desc)
```

### IRecommendationEngine

Code that consumes the engine should depend on `IRecommendationEngine`
(namespace `SqlIndexAdvisor.Core.Engine`), which has a single member:

```csharp
IReadOnlyList<IndexRecommendation> Analyze(ExecutionPlan plan);
```

- `plan` must not be `null`; otherwise `ArgumentNullException` is thrown.
- The result is empty when no rule applies. Treat every recommendation as
  advisory and check it against your workload before changing a schema.

Depend on the interface for dependency injection or for test fakes.
`RecommendationEngine` is the only implementation in this repository.

### Constructors and rule sets

| Constructor | Rules that run |
| --- | --- |
| `new RecommendationEngine()` | `DefaultRules.All()`: `EngineHintRule`, `FullScanWithFilterRule`, `ImplicitConversionRule`, with default `RecommendationOptions`. |
| `new RecommendationEngine(RecommendationOptions options)` | The same default rules, with `MinTableRows` and `MaxSelectivity` applied to the scan rules. |
| `new RecommendationEngine(IEnumerable<IIndexRule> rules)` | Exactly the rules you pass. Null entries are ignored. |

Rules are independent and must not have side effects. Overlap between their
output is resolved by `RecommendationMerger`, not by the rules.

### Example usage

Parse a plan, analyze it with the default rules through the interface, and keep
only medium-or-higher confidence suggestions. `Parse` returns one plan per
statement, so this sample analyzes the first one:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;

IReadOnlyList<ExecutionPlan> plans = new PlanParserFactory().Parse(File.ReadAllText("plan.sqlplan"));

IRecommendationEngine engine = new RecommendationEngine(new RecommendationOptions { MinTableRows = 10_000 });
IReadOnlyList<IndexRecommendation> recommendations = engine.Analyze(plans[0]);

foreach (var rec in recommendations.Where(r => r.Confidence >= Confidence.Medium))
{
    Console.WriteLine($"{rec.Table} ({string.Join(", ", rec.KeyColumns)}): ~{rec.EstimatedImpactPercent:F1}%");
}
```

To run only your own rules, pass them to the constructor. This rule flags every
scan that carries a filter, with low confidence and no impact estimate:

```csharp
using System.Collections.Generic;
using System.Linq;
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;

sealed class FilteredScanRule : IIndexRule
{
    public string Name => "filtered-scan";

    public IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan)
    {
        foreach (var node in plan.Nodes.Where(n => n.IsScan && n.TableName is not null && n.PredicateColumns.Count > 0))
        {
            yield return new IndexRecommendation
            {
                Table = node.TableName!,
                KeyColumns = node.PredicateColumns.ToList(),
                Confidence = Confidence.Low,
                Reasons = { $"{node.Operator} on {node.TableName} filters on {string.Join(", ", node.PredicateColumns)}." },
            };
        }
    }
}

IRecommendationEngine engine = new RecommendationEngine(new IIndexRule[] { new FilteredScanRule() });
```

Notes:

- The engine analyzes one plan at a time. For a file with several statements,
  call `Analyze` once per plan, as shown in the directory scenario above.
- The engine does not know which indexes already exist, so it can suggest an
  index that is already there. See the limitations below.
- Ordering is stable for equal keys, so the output is deterministic for the same
  plan and rule set.

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


## ExecutionPlan

Represents a normalized execution plan that both SQL Server XML and PostgreSQL JSON plans are flattened into. This common shape allows the rules engine to work with a single data structure regardless of the source dialect.

The `ExecutionPlan` contains metadata about the query (dialect, statement text, estimated cost), the plan tree (`Nodes`), and any engine-provided missing index hints (`EngineMissingIndexes`).


**Example usage**

```csharp
using System;
using System.IO;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;

class Example
{
    static void Main()
    {
        var factory = new PlanParserFactory();
        
        // Parse a SQL Server execution plan
        string sqlServerPlan = File.ReadAllText("samples/sqlserver_orders_scan.sqlplan");
        
        if (factory.TryParse(sqlServerPlan, out ExecutionPlan? plan))
        {
            Console.WriteLine($"Plan dialect: {plan.Dialect}");
            Console.WriteLine($"Statement: {plan.StatementText}");
            Console.WriteLine($"Total cost: {plan.EstimatedTotalCost:F3}");
            Console.WriteLine($"Nodes in plan: {plan.Nodes.Count}");
            Console.WriteLine($"Missing index hints: {plan.EngineMissingIndexes.Count}");
            
            // Access scan nodes
            var scanNodes = plan.Nodes.Where(n => n.IsScan).ToList();
            Console.WriteLine($"Scan operations: {scanNodes.Count}");
            
            // Analyze highest cost node
            var highestCostNode = plan.Nodes
                .Where(n => n.RelativeCost > 0)
                .OrderByDescending(n => n.RelativeCost)
                .FirstOrDefault();
            
            if (highestCostNode != null)
            {
                Console.WriteLine($"Highest cost node: {highestCostNode.Operator}");
                Console.WriteLine($"  Table: {highestCostNode.TableName}");
                Console.WriteLine($"  Cost: {highestCostNode.RelativeCost:P0}");
                Console.WriteLine($"  Rows: {highestCostNode.EstimatedRows:N0}");
                Console.WriteLine($"  Predicate columns: {string.Join(", ", highestCostNode.PredicateColumns)}");
                Console.WriteLine($"  Output columns: {string.Join(", ", highestCostNode.OutputColumns)}");
            }
            
            // Access engine missing index hints
            foreach (var hint in plan.EngineMissingIndexes)
            {
                Console.WriteLine($"\nEngine missing index hint:");
                Console.WriteLine($"  Table: {hint.Table}");
                Console.WriteLine($"  Impact: {hint.ImpactPercent:P1}");
                Console.WriteLine($"  Equality columns: {string.Join(", ", hint.EqualityColumns)}");
                Console.WriteLine($"  Inequality columns: {string.Join(", ", hint.InequalityColumns)}");
                Console.WriteLine($"  Include columns: {string.Join(", ", hint.IncludeColumns)}");
            }
        }
    }
}
```

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

## IndexRecommendation

`IndexRecommendation` represents a single indexing opportunity discovered by the recommendation engine. It captures the table and columns to index, the estimated performance benefit, and the reasoning behind the suggestion so you can quickly validate whether the index is appropriate for your workload.

Each recommendation is produced by an `IIndexRule` (e.g., `EngineHintRule` or `FullScanWithFilterRule`) and contains enough information to generate a `CREATE INDEX` statement via the `ToCreateStatement` helper.

**Example usage**

```csharp
using System;
using System.Collections.Generic;
using SqlIndexAdvisor.Core.Model;

class Example
{
    static void Main()
    {
        // Create a recommendation for a frequently filtered orders table
        var recommendation = new IndexRecommendation
        {
            Table = "dbo.Orders",
            KeyColumns = new List<string> { "Status", "CreatedAt" },
            IncludeColumns = new List<string> { "CustomerId", "Total", "ShippingAddress" },
            EstimatedImpactPercent = 82.5,
            Confidence = Confidence.High,
            Reasons = new List<string>
            {
                "Clustered Index Scan on dbo.Orders carries a filter on (Status, CreatedAt) and is ~100% of statement cost.",
                "Adding this index would eliminate the scan and allow the query to use an index seek."
            },
            SuggestedName = "IX_Orders_Status_CreatedAt"
        };

        // Generate the CREATE INDEX statement
        string createStatement = recommendation.ToCreateStatement();
        Console.WriteLine(createStatement);
        /*
        Output:
        CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (CustomerId, Total, ShippingAddress);
        */

        // Access properties
        Console.WriteLine($"Table: {recommendation.Table}");
        Console.WriteLine($"Key columns: {string.Join(", ", recommendation.KeyColumns)}");
        Console.WriteLine($"Include columns: {string.Join(", ", recommendation.IncludeColumns)}");
        Console.WriteLine($"Estimated impact: {recommendation.EstimatedImpactPercent:P1}");
        Console.WriteLine($"Confidence: {recommendation.Confidence}");
        Console.WriteLine($"Suggested name: {recommendation.SuggestedName}");
    }
}
```

## RecommendationEngineTestsExtensions

`RecommendationEngineTestsExtensions` provides a set of extension methods for testing index recommendation scenarios. These methods help you create test execution plans, build index recommendations, and assert on recommendation properties in a fluent and readable way. The extensions are particularly useful when writing unit tests for custom index rules or recommendation engine behavior.

**Example usage**

```csharp
using System;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Tests;
using Xunit;

class Example
{
    static void Main()
    {
        // Create test execution plans for different scenarios
        
        // 1️⃣ Create a sequential scan plan (Postgres)
        var seqScanPlan = "users".CreateSeqScanPlan(PlanDialect.Postgres);
        Console.WriteLine($"Created sequential scan plan with {seqScanPlan.Nodes.Count} node(s).");
        
        // 2️⃣ Create a clustered index scan plan (SQL Server)
        var clusteredScanPlan = "dbo.Orders".CreateClusteredIndexScanPlan(PlanDialect.SqlServer);
        Console.WriteLine($"Created clustered index scan plan with {clusteredScanPlan.Nodes.Count} node(s).");
        
        // 3️⃣ Create an index recommendation directly
        var recommendation = "dbo.Orders".CreateIndexRecommendation(
            keyColumns: new[] { "Status", "CreatedAt" },
            includeColumns: new[] { "CustomerId", "Total" }
        );
        
        Console.WriteLine($"Created recommendation for table: {recommendation.Table}");
        Console.WriteLine($"Key columns: {string.Join(", ", recommendation.KeyColumns)}");
        Console.WriteLine($"Include columns: {string.Join(", ", recommendation.IncludeColumns)}");
        
        // 4️⃣ Generate a CREATE INDEX statement and extract its components
        string createStatement = recommendation.ToCreateStatement();
        Console.WriteLine($"\nGenerated statement: {createStatement}");
        
        string indexName = createStatement.GetIndexName();
        string tableName = createStatement.GetTableName();
        var keyColumns = createStatement.GetKeyColumns();
        var includeColumns = createStatement.GetIncludeColumns();
        
        Console.WriteLine($"Index name: {indexName}");
        Console.WriteLine($"Table name: {tableName}");
        Console.WriteLine($"Key columns: {string.Join(", ", keyColumns)}");
        Console.WriteLine($"Include columns: {string.Join(", ", includeColumns)}");
        
        // 5️⃣ Use fluent assertions (in test context)
        var assertion = Assert.Simple();
        recommendation.HasConfidence(Confidence.High, recommendation);
        recommendation.HasKeyColumns(new[] { "Status", "CreatedAt" }, recommendation);
        recommendation.HasIncludeColumns(new[] { "CustomerId", "Total" }, recommendation);
    }
}
```

## RecommendationMergerTests

`RecommendationMergerTests` covers the merge/de-duplication step that collapses overlapping index suggestions before they are ranked. It verifies that suggestions for the same table whose key columns form a prefix chain are merged into a single wider recommendation, while suggestions targeting different tables - or overlapping on columns that are not a prefix of each other - are kept separate. The edge cases are guarded too: an empty input yields an empty list and a lone recommendation passes through unchanged.

**Example usage**

```csharp
using System;
using SqlIndexAdvisor.Tests;

class Example
{
    static void Main()
    {
        // Instantiate the test suite for the recommendation merge step.
        var tests = new RecommendationMergerTests();

        // 1️⃣ Key columns that extend each other (prefix) merge into one index.
        tests.Merge_WithPrefixColumns_MergesCorrectly();

        // 2️⃣ Identical key columns collapse into a single merged recommendation.
        tests.Merge_WithSameColumns_MergesCorrectly();

        // 3️⃣ Suggestions for different tables are never merged.
        tests.Merge_WithDifferentTables_DoesNotMerge();

        // 4️⃣ Partially overlapping (non-prefix) columns stay separate.
        tests.Merge_WithNonPrefixColumns_DoesNotMerge();

        // 5️⃣ Merging an empty list returns an empty list.
        tests.Merge_EmptyList_ReturnsEmptyList();

        // 6️⃣ A single recommendation passes through unchanged.
        tests.Merge_SingleRecommendation_ReturnsSame();
    }
}
```

## ReportRendererTests

The `ReportRendererTests` class verifies the correctness of the `ReportRenderer` class's text and JSON output for index recommendations. It tests scenarios such as empty recommendation lists, single and multiple recommendations, proper inclusion of table and column names, and validation of JSON structure and impact disclaimers.

**Example usage**

```csharp
using SqlIndexAdvisor.Tests;
using Xunit;

class Example
{
    static void Main()
    {
        var tests = new ReportRendererTests();
        tests.RenderText_EmptyRecommendations_ReturnsNoRecommendationsMessage();
        tests.RenderText_SingleRecommendation_IncludesTableName();
        // Additional test methods can be called similarly to verify other rendering behaviors.
    }
}
```

## PlanParserFactoryExtensionsTests

The `PlanParserFactoryExtensionsTests` class contains unit tests for the extension methods of `PlanParserFactory`, covering `TryParse`, `ParseMany`, `CanParse`, `GetRegisteredParserNames`, `GetRegisteredParsers` and `ParseWith`. It verifies correct behavior for valid and invalid inputs, null arguments, and edge cases.

**Example usage**

```csharp
using SqlIndexAdvisor.Tests;
using Xunit;

class Example
{
    static void Main()
    {
        var tests = new PlanParserFactoryExtensionsTests();
        tests.TryParse_ValidXml_ReturnsTrueAndPlan();
        tests.TryParse_ValidJson_ReturnsTrueAndPlan();
        // Additional test methods can be called similarly to verify other behaviors.
    }
}
```

## ArgsParserTests

The `ArgsParserTests` class contains unit tests for the `ArgsParser.Parse` method, which handles command-line argument parsing for the sql-index-advisor CLI. It verifies correct handling of help flags, format specification, minimum impact thresholds, stdin usage, file path normalization, and various error conditions.

**Example usage**

```csharp
using SqlIndexAdvisor.Core.ArgsParsing;
using System;

// Parse empty arguments (shows help)
var result = ArgsParser.Parse(Array.Empty<string>());
Console.WriteLine(result.ShouldShowHelp); // True

// Parse with format and minimum impact
result = ArgsParser.Parse(new[] { "--format", "json", "--min-impact", "25.5" });
Console.WriteLine(result.Format); // json
Console.WriteLine(result.MinImpact); // 25.5

// Parse file path with stdin flag
result = ArgsParser.Parse(new[] { "--stdin", "--format", "csv" });
Console.WriteLine(result.UseStdin); // True
Console.WriteLine(result.Format); // csv

// Parse a plan file path
result = ArgsParser.Parse(new[] { "query_plan.sqlplan" });
Console.WriteLine(result.Path); // Absolute path to query_plan.sqlplan
Console.WriteLine(result.Format); // text (default)
```

## PlanParserFactoryJsonExtensionsTests

The `PlanParserFactoryJsonExtensionsTests` class verifies the JSON serialization and deserialization functionality of the `PlanParserFactory` class through its extension methods. It tests serialization to JSON with and without indentation, deserialization from valid and invalid JSON strings, and proper handling of null inputs.

**Example usage**

```csharp
using SqlIndexAdvisor.Core.Parsing;
using SqlIndexAdvisor.Tests;
using Xunit;

class Example
{
    static void Main()
    {
        var tests = new PlanParserFactoryJsonExtensionsTests();
        
        // Test serialization to JSON (non-indented by default)
        var factory = new PlanParserFactory();
        string json = factory.ToJson(); 
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.DoesNotContain("\n", json); // Non-indented JSON should not contain line breaks
        
        // Test serialization with indentation
        string indentedJson = factory.ToJson(indented: true);
        Assert.False(string.IsNullOrWhiteSpace(indentedJson));
        Assert.Contains("\n", indentedJson); // Indented JSON should contain line breaks
        
        // Test deserialization from valid JSON
        var deserialized = PlanParserFactoryJsonExtensions.FromJson(json);
        Assert.NotNull(deserialized);
        Assert.IsType<PlanParserFactory>(deserialized);
        
        // Test TryFromJson with valid JSON
        bool success = PlanParserFactoryJsonExtensions.TryFromJson(json, out var factoryFromTry);
        Assert.True(success);
        Assert.NotNull(factoryFromTry);
        
        // Test TryFromJson with invalid JSON
        bool successInvalid = PlanParserFactoryJsonExtensions.TryFromJson("invalid json", out var factoryInvalid);
        Assert.False(successInvalid);
        Assert.Null(factoryInvalid);
    }
}
```

## IndexRecommendationExtensionsTests

The `IndexRecommendationExtensionsTests` class contains unit tests for the extension methods on `IndexRecommendation`, covering methods such as `ContainsColumn`, `GetTotalColumnCount`, `ToDisplayString`, and `ToSummaryString`. It verifies correct behavior for various inputs, including edge cases like null or empty values.

**Example usage**

```csharp
using SqlIndexAdvisor.Tests;
using Xunit;

class Example
{
    static void Main()
    {
        var tests = new IndexRecommendationExtensionsTests();
        tests.ContainsColumn_WithExistingKeyColumn_ReturnsTrue();
        tests.ContainsColumn_WithExistingIncludeColumn_ReturnsTrue();
        // Additional test methods can be called similarly to verify other behaviors.
    }
}
```

## IndexRecommendationValidationTests

The `IndexRecommendationValidationTests` class verifies the validation logic for `IndexRecommendation` objects. It tests that validation correctly identifies issues like null/empty table names, missing key columns, invalid impact percentages, and whitespace-only strings, while allowing null values for optional fields like include columns and reasons.

**Example usage**

```csharp
using SqlIndexAdvisor.Tests;
using Xunit;

class Example
{
    static void Main()
    {
        var tests = new IndexRecommendationValidationTests();
        tests.Validate_WithValidRecommendation_ReturnsEmptyList();
        tests.Validate_WithNullTable_ReturnsError();
        tests.IsValid_WithValidRecommendation_ReturnsTrue();
        tests.EnsureValid_WithValidRecommendation_DoesNotThrow();
        // Additional test methods can be called similarly to verify validation behaviors.
    }
}
```

## ExecutionPlanExtensionsTests

The `ExecutionPlanExtensionsTests` class contains unit tests for the extension methods on `ExecutionPlan`, covering methods such as `GetScanCandidates`, `GetTotalScanCost`, `GetScannedTables`, `GetPredicateColumns`, `GetIncludeCandidateColumns`, and others related to missing index hints and scan analysis. It verifies correct behavior for valid plans, empty plans, and null plans.

**Example usage**

```csharp
using SqlIndexAdvisor.Tests;
using Xunit;

class Example
{
    static void Main()
    {
        var tests = new ExecutionPlanExtensionsTests();
        tests.GetScanCandidates_WithValidPlan_ReturnsScanNodes();
        tests.GetTotalScanCost_WithValidPlan_ReturnsSumOfScanCosts();
        // Additional test methods can be called similarly to verify other behaviors.
    }
}
```

## ExecutionPlanValidationTests

The `ExecutionPlanValidationTests` class contains unit tests for validating `ExecutionPlan` instances using the `IExecutionPlanValidationTests` interface. It verifies that validation methods (`Validate`, `IsValid`, `EnsureValid`) correctly handle valid plans, null inputs, and various invalid conditions such as negative costs, null nodes, and invalid enum values.

**Example usage**

```csharp
using SqlIndexAdvisor.Tests;
using Xunit;

class Example
{
    static void Main()
    {
        var tests = new ExecutionPlanValidationTests();
        tests.Validate_HappyPath_ForEachMajorPublicMethod_ReturnsNoProblems();
        tests.Validate_NullInput_ThrowsArgumentNullException();
        // Additional test methods can be called similarly to verify validation behaviors.
    }
}
```

## RecommendationEngineJsonExtensionsTests

The `RecommendationEngineJsonExtensionsTests` class verifies the JSON serialization and deserialization functionality of the `RecommendationEngine` class through its extension methods. It tests serialization to JSON with and without indentation, deserialization from valid and invalid JSON strings, and proper handling of null inputs.

**Example usage**

```csharp
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Tests;
using Xunit;

class Example
{
    static void Main()
    {
        var tests = new RecommendationEngineJsonExtensionsTests();
        
        // Test serialization to JSON
        var engine = new RecommendationEngine();
        string json = engine.ToJson(); // Returns "{}"
        Assert.False(string.IsNullOrWhiteSpace(json));
        
        // Test serialization with indentation
        string indentedJson = engine.ToJson(indented: true);
        Assert.Contains("\n", indentedJson);
        
        // Test deserialization from valid JSON
        var deserialized = RecommendationEngineJsonExtensions.FromJson("{}");
        Assert.NotNull(deserialized);
        Assert.IsType<RecommendationEngine>(deserialized);
        
        // Test TryFromJson with valid JSON
        bool success = RecommendationEngineJsonExtensions.TryFromJson("{}", out var engineFromTry);
        Assert.True(success);
        Assert.NotNull(engineFromTry);
        
        // Test TryFromJson with invalid JSON
        bool successInvalid = RecommendationEngineJsonExtensions.TryFromJson("invalid json", out var engineInvalid);
        Assert.False(successInvalid);
        Assert.Null(engineInvalid);
    }
}
```

## FullScanWithFilterRuleTests

The `FullScanWithFilterRuleTests` class contains unit tests for the `FullScanWithFilterRule` class, which identifies table or index scans that have filter predicates and recommends indexes to eliminate those scans. It verifies the rule's behavior for various scan types (sequential, clustered index, index scan) and conditions (presence of predicate, cost levels, etc.).

**Example usage**

```csharp
using SqlIndexAdvisor.Tests;
using Xunit;

class Example
{
    static void Main()
    {
        var tests = new FullScanWithFilterRuleTests();
        tests.Evaluate_SeqScanWithFilterPredicate_ReturnsRecommendation();
        tests.Evaluate_ScanWithLowCost_ReturnsLowConfidence();
        // Additional test methods can be called similarly to verify other behaviors.
    }
}
```

## IndexRecommendationTests

The `IndexRecommendationTests` class exercises `IndexRecommendation` initialization, suggested index-name generation, and SQL `CREATE INDEX` statement generation. It also covers edge cases involving schemas, special characters, null collections, impact percentages, and source-node costs.

**Example usage**

```csharp
using SqlIndexAdvisor.Tests;

class Example
{
    static void Main()
    {
        var tests = new IndexRecommendationTests();

        tests.Constructor_WithRequiredProperties_InitializesCorrectly();
        tests.SuggestedName_WithSchemaQualifiedTable_ReturnsCorrectFormat();
        tests.ToCreateStatement_WithKeyAndIncludeColumns_ReturnsCorrectSql();
        tests.EstimatedImpactPercent_WithBoundaryValues_StoresCorrectly();
        tests.SourceNodeCost_WithValidValue_StoresCorrectly();
    }
}
```

## License

MIT.
