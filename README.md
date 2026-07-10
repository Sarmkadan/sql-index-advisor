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

## Impact estimate - what the number means

For engine hints it is simply the optimizer's reported impact. For the scan
rule it is the node's share of statement cost scaled by a rough selectivity
factor (how few rows the filter keeps versus how many it reads). It is a
ranking aid, nothing more.

## Project layout

```
src/SqlIndexAdvisor.Core   parsing, model, rules, reporting (the library)
src/SqlIndexAdvisor.Cli    thin command-line front end
tests/SqlIndexAdvisor.Tests xUnit tests for parsers and the engine
samples/                   example plans for both dialects
```

## Limits / not done yet

- No column-order awareness beyond equality-before-inequality. It will not
  reorder keys by selectivity because a plan alone does not carry that.
- It does not check whether a similar index already exists on the table - it
  only sees the single plan you give it.
- Postgres predicate parsing is string-based; unusual expression formatting can
  cause a column to be missed. When in doubt, read the reasons it prints.

## License

MIT.
