# CLAUDE.md

## Overview

sql-index-advisor is a .NET 10 CLI that parses SQL Server showplan XML or PostgreSQL `EXPLAIN (FORMAT JSON)` output and emits ranked index recommendations (text, json, html, csv, markdown).

## Build

```bash
dotnet build                      # solution: SqlIndexAdvisor.slnx
dotnet run --project src/SqlIndexAdvisor.Cli -- samples/sqlserver_orders_scan.sqlplan
dotnet run --project src/SqlIndexAdvisor.Cli -- samples/postgres_users_seqscan.json --format json
cat plan.json | dotnet run --project src/SqlIndexAdvisor.Cli -- --stdin
```

Exit codes: `0` ok, `1` usage/IO error, `2` plan could not be parsed (`PlanParseException`).

## Test

```bash
dotnet test                                                  # all (xUnit 2.9, coverlet)
dotnet test --filter "FullyQualifiedName~SqlServerParserTests"
```

Tests live in `tests/SqlIndexAdvisor.Tests`; plan fixtures in `tests/SqlIndexAdvisor.Tests/Fixtures/{SqlServer,PostgreSQL}` and `samples/`. Tests feed plans to Core as inline strings - no filesystem or console access from Core.

## Lint / format

No analyzers or CI configured. Style is enforced by `.editorconfig` only: 4-space indent, LF, Allman braces (`csharp_new_line_before_open_brace = all`), `System` usings first, final newline. `dotnet format` is the expected formatter. `Nullable` and `ImplicitUsings` are enabled in all projects.

## Layout

| Path | Role |
| --- | --- |
| `src/SqlIndexAdvisor.Cli/Program.cs` | Entry point (top-level statements). Arg parsing via `ArgsParser`, reads file/stdin, picks renderer, sets exit code. Only place with I/O. |
| `src/SqlIndexAdvisor.Core/Parsing/` | `IPlanParser` (`CanParse`/`Parse`), `PlanParserFactory` (first parser that claims the content wins), `SqlServerXmlPlanParser`, `PostgresJsonPlanParser`, `PredicateColumnScanner`. |
| `src/SqlIndexAdvisor.Core/Model/` | `ExecutionPlan` / `PlanNode` (flat node list with `Parent` pointers, `RelativeCost` 0..1), `IndexRecommendation`, `Recommendation`, validation and extension classes. |
| `src/SqlIndexAdvisor.Core/Rules/` | `IIndexRule` (`Name`, `Evaluate(ExecutionPlan)`), `PlanNodeVisitorBase`, concrete rules (`FullScanWithFilterRule`, `EngineHintRule`, `KeyLookupRule`, `MissingJoinIndexRule`, `ImplicitConversionRule`, `ExpensiveSortRule`), `DefaultRuleSet`. |
| `src/SqlIndexAdvisor.Core/Engine/` | `RecommendationEngine` runs rules, `RecommendationMerger` de-dups/ranks. |
| `src/SqlIndexAdvisor.Core/Reporting/` | `ReportRenderer` (static, `RenderText`/`RenderJson`), `IReportRenderer` + `HtmlReportRenderer`/`CsvReportRenderer`/`MarkdownReportRenderer`, `DdlRenderer` for `CREATE INDEX` DDL. |
| `src/SqlIndexAdvisor.Core/ArgsParsing/` | `ArgsParser` (CLI options, help text). |
| `docs/ARCHITECTURE.md` | Design rationale; read before changing the model or engine. |
| `build/` | Committed compiled output. Do not hand-edit; not a source of truth. |

Stray root-level files `SqlServerXmlPlanParser.cs` and `test_conflict.cs` are not part of any csproj (projects glob only their own folders); ignore them.

Pipeline: raw text -> `PlanParserFactory` -> `ExecutionPlan` -> `RecommendationEngine` (rules + merge) -> `IndexRecommendation[]` -> `IReportRenderer` -> stdout.

## Conventions

- Namespaces mirror folders: `SqlIndexAdvisor.Core.{Parsing,Model,Rules,Engine,Reporting,ArgsParsing}`. One public type per file, file named after the type.
- Interfaces prefixed `I`; rules end in `Rule`; renderers in `Renderer`; parsers in `PlanParser`.
- Magic strings (XML element/attribute names, JSON property names) go into sibling `*Constants.cs` static classes, e.g. `SqlServerXmlPlanParserConstants`, `PostgresJsonPlanParserConstants`.
- Helper logic is split into `*Extensions.cs` / `*JsonExtensions.cs` / `*Validation.cs` partials next to the type they extend; keep that split when adding helpers.
- Rules must be independent and side-effect free; overlap is resolved in `RecommendationMerger`, not in the rule. Rules compare `PlanNode.RelativeCost`, never absolute cost (SQL Server and Postgres units are not comparable).
- New dialect: implement `IPlanParser`, register via the `PlanParserFactory(IEnumerable<IPlanParser>)` overload. New rule: derive from `PlanNodeVisitorBase` or implement `IIndexRule`, add to `DefaultRuleSet`.
- Every `IndexRecommendation` must carry human-readable `Reasons`.
- Core must stay free of `Console` and `File` access.
- Tests: xUnit, class per subject named `<Subject>Tests` (plus `<Subject>TestsExtensions`/`...Constants` split mirroring source). Add fixtures under `Fixtures/<Dialect>/`.
- Commit messages: conventional prefixes (`feat:`, `refactor:`, `docs:`, `chore:`).
