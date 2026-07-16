# Architecture

This document describes how sql-index-advisor is put together and why. It is
grounded in the code as it exists today; if the code and this doc disagree,
the code wins and this doc needs fixing.

## Overview

The tool is a plan-in, recommendations-out pipeline:

```
raw plan text
    │
    ▼
PlanParserFactory ──picks──► IPlanParser (SqlServerXmlPlanParser | PostgresJsonPlanParser)
    │
    ▼
ExecutionPlan (normalized model)
    │
    ▼
RecommendationEngine ──runs──► IIndexRule[] (EngineHintRule, FullScanWithFilterRule)
    │        │
    │        └── merge / de-dup / rank
    ▼
IndexRecommendation[]
    │
    ▼
ReportRenderer (text | json) ──► stdout
```

Two projects, one test project:

| Project | Role |
| --- | --- |
| `src/SqlIndexAdvisor.Core` | Everything: parsing, model, rules, engine, reporting. No I/O, no console. |
| `src/SqlIndexAdvisor.Cli` | ~100-line front end: argument parsing, file/stdin reading, exit codes. |
| `tests/SqlIndexAdvisor.Tests` | xUnit tests for both parsers, the predicate scanner, and the engine. |

The Core library never touches the filesystem or the console. The CLI hands it
a string and prints whatever comes back. That is the whole layering rule, and
it is why the tests can exercise everything with inline plan literals.

## Components

### Model (`Core/Model`)

`ExecutionPlan` is the normalization target both dialects flatten into:

- `Dialect` - `SqlServer` or `Postgres`. Kept on the plan (not threaded
  through every call) because the only consumer that cares is
  `IndexRecommendation.ToCreateStatement`, which needs it for syntax.
- `Nodes` - a flat `List<PlanNode>`, one per operator. Each node keeps a
  `Parent` pointer instead of a child list. Rationale: every rule we have
  starts at a scan and looks upward (or doesn't look at all); a flat list plus
  parent pointers makes "iterate all scans" a plain `foreach` with no tree
  traversal code. The trade-off is that a future rule needing top-down
  traversal (e.g. "is this scan under a nested loop?") would have to invert
  the pointers or scan the list - acceptable until such a rule exists.
- `EngineMissingIndexes` - missing-index hints the optimizer itself emitted
  (SQL Server showplan carries these; Postgres has no equivalent). They are
  modeled separately from nodes because they are declarations, not operators.

`PlanNode.RelativeCost` is precomputed at parse time as the node's share of
statement cost (0..1). Rules never see absolute costs - SQL Server and
Postgres cost units are not comparable, but a *fraction of the statement* is,
and that one decision is what lets the same rules run on both dialects.

`IndexRecommendation` carries the suggestion plus `Reasons` (human-readable
strings explaining which evidence produced it). Reasons are first-class
because the numbers are heuristics; the output is only trustworthy if the
user can see the "why" and judge it.

### Parsing (`Core/Parsing`)

`IPlanParser` is two methods: `CanParse(string)` (cheap content sniff) and
`Parse(string)`. `PlanParserFactory` walks its parser list and picks the first
that claims the content; format detection is trivial in practice because
showplan XML starts with `<` and EXPLAIN JSON with `[`/`{`. The factory has a
constructor overload taking `IEnumerable<IPlanParser>` so a new dialect (MySQL
`EXPLAIN FORMAT=JSON`, say) plugs in without touching existing code.

**SqlServerXmlPlanParser** matches XML elements by *local name*, deliberately
ignoring the showplan namespace URI. Microsoft revs that URI between SQL
Server versions; binding to it would break the parser on every release for
zero benefit. It extracts operators (`RelOp`), their filter predicates, output
columns, and the `MissingIndexes` block.

**PostgresJsonPlanParser** walks the `Plans` tree from
`EXPLAIN (FORMAT JSON)`. Postgres does not give structured predicates - the
`Filter` / `Index Cond` fields are strings - so column extraction is delegated
to `PredicateColumnScanner`, a small regex tokenizer that grabs the identifier
to the left of a comparison operator. Predicates wrapping a column in a
function (`lower(name) = ...`) are *intentionally not matched*: such a
predicate is not sargable, so an index on the bare column would not help and
recommending one would be wrong. What looks like a scanner limitation is
actually the correct filter.

Parse failures throw `PlanParseException`, the one exception type the CLI
maps to a distinct exit code (2) so CI scripts can tell "bad plan file" from
"tool bug".

### Rules (`Core/Rules`)

`IIndexRule` is `Name` + `Evaluate(ExecutionPlan) -> IEnumerable<IndexRecommendation>`.
Rules are independent and stateless; they may emit overlapping suggestions
freely because overlap resolution is the engine's job, not theirs. That
separation keeps each rule ~50 lines and testable in isolation.

- **EngineHintRule** replays SQL Server's own missing-index hints as
  `High`-confidence recommendations. The engine costed these itself, so we do
  not second-guess them. Key ordering follows the standard guidance: equality
  columns first, then inequality.
- **FullScanWithFilterRule** is the workhorse and the only rule that does any
  real inference. It fires on a full scan (`Seq Scan`, `Table Scan`,
  `Clustered Index Scan`) that carries a filter and costs at least 10% of the
  statement. Filter columns become the key; the scan's other output columns
  become `INCLUDE` candidates so the suggested index can cover the query.
  Confidence is tiered off the node's cost share (>=60% High, >=30% Medium),
  and the impact estimate scales cost share by observed selectivity
  (rows out vs rows read), blended so even a weak filter credits the
  seek-vs-scan saving. All thresholds are constants in the rule - they are
  guesses, kept in one visible place rather than hidden in expressions.

### Engine (`Core/Engine`)

`RecommendationEngine` runs every rule, then merges. Two recommendations are
duplicates when they target the same table and one's key column list is a
prefix of the other's (case-insensitive). The wider one wins and absorbs the
narrower one's includes and reasons; impact and confidence take the max. This
handles the common case where the engine hint and the scan rule describe the
same index at different widths - without the prefix match the tool would emit
two near-identical CREATE INDEX statements and look silly.

Final ordering: confidence desc, then estimated impact desc.

The engine's default constructor wires the built-in rules; a second
constructor takes `IEnumerable<IIndexRule>`. Same pattern as the parser
factory: **poor-man's DI by constructor overload**. There is no container
because a two-project CLI does not need one; the overloads keep every
component swappable in tests, which is the only consumer that ever swaps them.

### Reporting (`Core/Reporting`)

`ReportRenderer` is a static class with two pure functions: `RenderText` for
humans, `RenderJson` for pipes (`jq`, CI gates). Both take the plan plus the
final recommendation list and return a string - no console access, so they are
snapshot-testable. The JSON shape is an anonymous type serialized indented;
it is the tool's de-facto output contract, so changes to it are breaking.

### CLI (`src/SqlIndexAdvisor.Cli`)

`Program.cs` is a single top-level file: hand-rolled arg loop (no
System.CommandLine dependency for four options), file-or-stdin input,
`--min-impact` post-filter, and the exit-code mapping:

- `0` - success
- `1` - usage or I/O error
- `2` - content was not a recognizable plan (`PlanParseException`)

The `--min-impact` filter lives in the CLI, not the engine, because it is a
presentation concern - the engine's job is to report everything it found.

## Key design decisions, condensed

1. **Normalize plans into one model, compare only relative costs.** This is
   the load-bearing decision; everything downstream is dialect-agnostic
   except CREATE INDEX syntax.
2. **Rules emit freely, engine de-dups.** Rules stay small and ignorant of
   each other; the merge policy (prefix match, wider wins) lives in exactly
   one place.
3. **Parent pointers, flat node list.** Optimized for the access pattern the
   rules actually have; revisit if a top-down rule appears.
4. **String-scanning Postgres predicates, skipping function-wrapped columns.**
   Regex over `Filter` text is fragile in theory, but the failure mode is a
   missed column (a missed recommendation), never a wrong one - and skipping
   non-sargable predicates is required behavior anyway.
5. **No DI container, no CLI framework, no logging library.** The dependency
   list is empty on purpose; the tool is meant to be auditable in one sitting.

## Extension points

- **New dialect**: implement `IPlanParser`, pass it to
  `PlanParserFactory(IEnumerable<IPlanParser>)`. Nothing else changes as long
  as the parser fills `RelativeCost` and predicate/output columns.
- **New rule**: implement `IIndexRule`, pass it to
  `RecommendationEngine(IEnumerable<IIndexRule>)`. The merge step handles
  overlap with existing rules automatically.
- **New output format**: add a renderer function and a `--format` case in the
  CLI.

## Known limitations

- Key column order is whatever the predicate/hint produced (equality before
  inequality for engine hints); there is no selectivity-based reordering
  because a plan alone does not carry per-column statistics.
- The tool sees one plan at a time. It cannot know an equivalent index
  already exists, nor weigh write amplification across a workload.
- Postgres predicate extraction can miss columns under unusual expression
  formatting (see decision 4 - it fails safe, but it fails silent).
- `PlanNode.Parent` is populated by the Postgres parser only (the SQL Server
  parser flattens without linking) and no current rule uses it; it exists for
  the "scan under a join" class of rules that has not been written yet. If
  such a rule lands, the SQL Server parser must start setting it too.
