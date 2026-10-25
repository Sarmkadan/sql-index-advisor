# Plan Parsing

This document describes the plan parsing subsystem in `src/SqlIndexAdvisor.Core/Parsing/`: how raw
plan text becomes one `ExecutionPlan` per statement. It covers the interfaces, the factory and its
extension methods, the two dialect parsers, and the predicate column scanner used by the PostgreSQL
parser. It is grounded in the code; if this document and the code disagree, the code wins and this
document needs fixing. The model the parsers produce is described in [ExecutionPlan.md](ExecutionPlan.md).
The stage that consumes the output is described in [recommendation-engine.md](recommendation-engine.md).

## Pipeline position

```
raw text -> IPlanParserFactory / PlanParserFactory -> IPlanParser (sniffed) -> ExecutionPlan[] -> RecommendationEngine
```

The CLI (`src/SqlIndexAdvisor.Cli/Program.cs`) rejects blank input with a `PlanParseException` of its
own, then calls `new PlanParserFactory().Parse(content)`. A `PlanParseException` reaching the top
level makes the process exit with code `2`.

## Files at a glance

| File | Type(s) | Role |
| --- | --- | --- |
| `IPlanParser.cs` | `IPlanParser`, `PlanParseException` | Per-dialect parser contract; the exception type used across the subsystem. |
| `IPlanParserFactory.cs` | `IPlanParserFactory` | Factory contract: resolve a parser for content, or parse it. |
| `PlanParserFactory.cs` | `PlanParserFactory` | Default factory. Sniffs content against an ordered list of parsers. |
| `PlanParserFactoryExtensions.cs` | `PlanParserFactoryExtensions` | Extension methods on `PlanParserFactory` (`TryParse`, `ParseMany`, `CanParse`, `ParseWith`, and parser-name helpers). |
| `PlanParserFactoryExtensionsConstants.cs` | `PlanParserFactoryExtensionsConstants` (internal) | Error message used by `ParseWith`. |
| `SqlServerXmlPlanParser.cs` | `SqlServerXmlPlanParser` | Parses SQL Server showplan XML. |
| `SqlServerXmlPlanParserConstants.cs` | `SqlServerXmlPlanParserConstants` (internal) | XML element/attribute names, usage tokens, limits, messages. |
| `PostgresJsonPlanParser.cs` | `PostgresJsonPlanParser` | Parses PostgreSQL `EXPLAIN (FORMAT JSON)` output. |
| `PostgresJsonPlanParserConstants.cs` | `PostgresJsonPlanParserConstants` (internal) | JSON property names used by the Postgres parser. |
| `PredicateColumnScanner.cs` | `PredicateColumnScanner` | Extracts column names from Postgres predicate text. |

## IPlanParser

```csharp
public interface IPlanParser
{
    bool CanParse(string content);
    IReadOnlyList<ExecutionPlan> Parse(string content, CancellationToken cancellationToken = default);
}
```

- `CanParse` is a cheap format sniff. It returns `true` when the parser thinks it can handle the
  content. It does not validate the whole document.
- `Parse` returns one `ExecutionPlan` per statement, in input order. The contract says the list is
  never empty. It throws `PlanParseException` for malformed content or content with no executable
  plan. It throws `OperationCanceledException` when the token is canceled.

`PlanParseException` is defined in the same file. It is the only exception type callers are expected
to handle for bad plan input. Its constructor takes a message and an optional inner exception.

## IPlanParserFactory

```csharp
public interface IPlanParserFactory
{
    IPlanParser Resolve(string content);
    IReadOnlyList<ExecutionPlan> Parse(string content);
}
```

The interface is deliberately small. It only declares resolution and parsing. `PlanParserFactory` is
the only implementation in the repository, and it has more members (`TryResolve`,
`RegisteredParserNames`, and the internal `GetRegisteredParsersInternal`) that are not on the
interface. The extension methods in `PlanParserFactoryExtensions` are declared on the concrete
`PlanParserFactory`, not on `IPlanParserFactory`. Code that holds an `IPlanParserFactory` can call
only `Resolve` and `Parse`.

## PlanParserFactory

### Registration and order

- `new PlanParserFactory()` registers `SqlServerXmlPlanParser`, then `PostgresJsonPlanParser`.
- `new PlanParserFactory(IEnumerable<IPlanParser>)` registers the given parsers, in the order given.
  The sequence is copied into a list at construction. `null` throws `ArgumentNullException`.

Resolution is first-match. `Resolve` returns the first registered parser whose `CanParse` returns
`true`. The built-in parsers are mutually exclusive in practice: XML starts with `<`, and JSON starts
with `[` or `{`. Order therefore matters only for custom parser sets.

### Members

| Member | Behavior |
| --- | --- |
| `Resolve(content)` | Returns the first parser whose `CanParse` accepts `content`. Throws `PlanParseException` when none does. The message names the expected formats and the parsers tried. |
| `Parse(content)` | `Resolve(content).Parse(content)`. |
| `TryResolve(content, out parser)` | Like `Resolve`, but returns `false` instead of throwing when no parser matches. |
| `RegisteredParserNames` | The `GetType().Name` of each registered parser, in order. |

### Empty and whitespace input

- Whitespace-only content matches no parser, so `Resolve` throws `PlanParseException`.
- An empty string is different. The built-in `CanParse` implementations throw `ArgumentException` on
  empty input instead of returning `false`. The first parser's `CanParse` therefore throws, and the
  exception escapes `Resolve`, `Parse`, `TryResolve` and the extension methods below as an
  `ArgumentException`, not a `PlanParseException`. The CLI avoids this path because it checks for
  blank input first.

## PlanParserFactoryExtensions

All methods extend `PlanParserFactory`.

| Method | Behavior |
| --- | --- |
| `TryParse(content, out plans)` | Calls `Parse`. Returns `true` with the plans, or `false` with `null` when `PlanParseException` is thrown. Other exceptions propagate. |
| `CanParse(content)` | `true` if `Resolve` succeeds, `false` on `PlanParseException`. It does not parse the document. Other exceptions propagate. |
| `ParseMany(IEnumerable<(string SourceId, string Content)>)` | Lazily yields `(SourceId, Plan)` for every statement of every input that parses. Inputs that fail with `PlanParseException` are skipped silently. A multi-statement input yields one tuple per statement, all with the same `SourceId`. The iterator is lazy: nothing is parsed until the result is enumerated. |
| `ParseWith(content, selector)` | Passes the registered parsers to `selector` and parses with the one it returns. A `null` result throws `PlanParseException` with `NoParserSelectedErrorMessage`. Use it for custom selection logic that `Resolve` cannot express. |
| `GetRegisteredParsers()` | The registered `IPlanParser` instances, as a read-only list. |
| `GetRegisteredParserNames()` | The same parsers as type names. |

`ParseMany` is the only extension that enumerates lazily. `ParseWith` and `TryParse` run immediately.

## SqlServerXmlPlanParser

Parses SQL Server showplan XML, the format produced by `SET STATISTICS XML ON` or by saving an
execution plan from SSMS.

### Detection

`CanParse` trims leading whitespace, requires the first character to be `<`, and then requires the
text to contain `ShowPlanXML` or `StmtSimple`, compared case-insensitively. The namespace is ignored.
Elements are matched by local name, so the parser keeps working across schema version changes.

### Parsing

1. **Hardened XML read.** The document is read with `DtdProcessing.Prohibit` and a `null`
   `XmlResolver`, which blocks XXE. Limits are `MaxCharactersInDocument` = 10,000,000 and
   `MaxCharactersFromEntities` = 1,000,000.
2. **Error mapping.** An `XmlException` becomes a `PlanParseException` whose message gives the line
   and position and suggests that the file may be a `.sqlplan` wrapper. Any other exception becomes
   `PlanParseException("Content is not well-formed XML.")`.
3. **Statement selection.** The parser collects every `StmtSimple` element that has a `QueryPlan`
   descendant. Each such statement becomes its own plan.
4. **No statements.** When no statement carries a plan, the whole document becomes one plan. In that
   case the statement text is empty and the statement cost is zero.
5. **Per-statement build.** `BuildPlan` takes the operators (`RelOp`) whose nearest `StmtSimple`
   ancestor is that statement. Operators, costs, text and missing-index hints from one statement
   never leak into another.

### Mapping to PlanNode and ExecutionPlan

| Source | Target |
| --- | --- |
| `StmtSimple/@StatementText` | `ExecutionPlan.StatementText` |
| `StmtSimple/@StatementSubTreeCost` | `ExecutionPlan.EstimatedTotalCost` |
| `RelOp/@PhysicalOp`, else `@LogicalOp`, else `"Unknown"` | `PlanNode.Operator` |
| `RelOp/@EstimateRows` | `PlanNode.EstimatedRows` |
| `RelOp/@EstimatedRowsRead` | `PlanNode.EstimatedRowsRead` |
| `RelOp/@TableCardinality` | `PlanNode.TableCardinality` |
| `RunTimeCountersPerThread/@ActualRows`, summed | `PlanNode.ActualRows` |
| `RunTimeCountersPerThread/@ActualRowsRead`, summed | `PlanNode.ActualRowsRead` |
| `RelOp/@EstimatedTotalSubtreeCost` / statement cost | `PlanNode.RelativeCost` (0 when the statement cost is not positive) |
| First `Object` descendant, `@Schema.@Table` | `PlanNode.TableName` |
| First `Object` descendant, `@Index` | `PlanNode.IndexName` |
| `Predicate` → `ColumnReference/@Column` | `PlanNode.PredicateColumns` (deduplicated) |
| `OutputList` → `ColumnReference/@Column` | `PlanNode.OutputColumns` (deduplicated) |
| Nearest `RelOp` ancestor | `PlanNode.Parent` |
| `MissingIndexGroup` / `MissingIndex` | `ExecutionPlan.EngineMissingIndexes` |

Notes:

- Predicate and output columns are read only from the operator's own elements. The parser skips
  nested `RelOp` subtrees, so a parent operator does not inherit its children's columns.
- `[` and `]` are trimmed from every identifier. Schema and table are joined with `.`.
- `ActualRows` and `ActualRowsRead` are zero when the plan has no runtime counters, which is the case
  for estimated-only plans.
- Missing-index column groups are mapped by `Usage`: `EQUALITY` goes to equality columns,
  `INEQUALITY` to inequality columns, and any other value to include columns. `Impact` is copied to
  `ImpactPercent`.
- Numbers are parsed with the invariant culture. Values that fail to parse, and `NaN` or infinity,
  become `0`. This keeps cost ranking from being poisoned.

The SQL Server parser does **not** use `PredicateColumnScanner`. Predicate columns come directly from
the `ColumnReference` attributes in the XML.

## PostgresJsonPlanParser

Parses `EXPLAIN (FORMAT JSON)` and `EXPLAIN (ANALYZE, FORMAT JSON)` output. PostgreSQL plans do not
include missing-index hints. Index recommendations are derived from the scan and filter nodes instead.

### Detection

`CanParse` trims leading whitespace, requires the first character to be `[` or `{`, and then requires
the text to contain `"Node Type"` or `"Plan"` (quoted, case-insensitive). Empty input throws
`ArgumentException`, as noted above.

### Parsing

1. **Size limit.** The UTF-8 byte count must not exceed 10 MiB. Larger input throws `PlanParseException`.
2. **JSON read.** `JsonDocument.Parse` runs with `AllowTrailingCommas = true`,
   `CommentHandling = Skip` and `MaxDepth = 1000`. `JsonException` becomes
   `PlanParseException("Content is not valid JSON.")`.
3. **Shape.**
   - A top-level object is one statement.
   - A top-level array is one statement per entry. An empty array throws `PlanParseException`.
   - Each entry must be an object with a `Plan` object. `Query Text`, when present, becomes the
     statement text.
4. **Tree walk.** `Walk` visits `Plan`, then recursively its `Plans` children, setting `Parent` on
   each node. It enforces a nesting limit of 1000 levels as a second guard, and it throws on non-object
   nodes.

### Mapping to PlanNode and ExecutionPlan

| Source | Target |
| --- | --- |
| `Query Text` | `ExecutionPlan.StatementText` (empty string when absent) |
| Root `Plan/Total Cost` | `ExecutionPlan.EstimatedTotalCost` |
| `Node Type` | `PlanNode.Operator` (`"Unknown"` when absent) |
| `Relation Name` | `PlanNode.TableName` |
| `Index Name` | `PlanNode.IndexName` |
| `Plan Rows` | `PlanNode.EstimatedRows` |
| `Actual Rows` × `Actual Loops` | `PlanNode.ActualRows` (zero without ANALYZE) |
| (`Actual Rows` + `Rows Removed by Filter` + `Rows Removed by Index Recheck`) × loops | `PlanNode.ActualRowsRead` (zero without ANALYZE) |
| Node `Total Cost` / root `Total Cost` | `PlanNode.RelativeCost` (0 when the root cost is not positive) |
| `Filter`, `Index Cond`, `Recheck Cond`, `Hash Cond` | `PlanNode.PredicateColumns`, via `PredicateColumnScanner` |
| `Output` (string array) | `PlanNode.OutputColumns` |

Notes:

- `Actual Loops` is treated as at least 1. A missing or malformed value counts as one loop.
- Missing numeric properties read as `0`. Missing or non-string text properties read as `null`.
- The Postgres parser does not set `EngineMissingIndexes`. It stays at the model default.
- Predicate columns are passed to the scanner with `isPostgres: true`, so they come back double-quoted.
  Duplicates across the four predicate keys are removed.

## PredicateColumnScanner

```csharp
public static partial class PredicateColumnScanner
{
    public static IEnumerable<string> Scan(string expression);
    public static IEnumerable<string> Scan(string expression, bool isPostgres);
}
```

Extracts the column names that appear as the left-hand side of a comparison in a predicate string.
Example: `(status = 'open'::text) AND ("Country" IN ('PL'))` yields `status` and `Country`
(quoted when `isPostgres` is true).

### Dialect detection

`Scan(expression)` decides the dialect itself. It returns Postgres mode when the text contains
`"Node Type"` or `"Plan"`. `Scan(expression, isPostgres)` skips the check and uses the flag given.
A `null` expression returns an empty result in both overloads.

### Algorithm

1. **Match.** A generated regex matches an identifier, optionally qualified (`t.col`) and optionally
   double-quoted, followed by a comparison operator. The operators are `=`, `<>`, `!=`, `<=`, `>=`,
   `<`, `>`, `~`, `LIKE`, `ILIKE`, `IS`, `IS NOT`, `[NOT] IN`, and `[NOT] BETWEEN`. The match must be
   followed by whitespace, `(`, `'`, or end of text.
2. **Column text.** The quoted group is used when present, otherwise the bare identifier.
3. **Function filter.** Identifiers that appear inside a call to a known SQL function are skipped. The
   known names are listed in `FunctionNames`, for example `UPPER`, `COALESCE`, `DATE_TRUNC`, and
   `COUNT`. Unknown function names are not treated as functions, so their arguments are still scanned.
4. **Arithmetic filter.** A candidate preceded by `*`, `/` or `+` in an arithmetic context (the
   character before the operator is a letter, digit, underscore or `)`) is skipped. A leading `-` is
   not treated this way because it may be a unary minus.
5. **Noise filter.** Keywords `AND`, `OR`, `NOT`, `NULL`, `TRUE`, `FALSE`, `ANY` and `ALL` are dropped.
6. **Output.** In Postgres mode each name is wrapped in double quotes. The scanner does not
   deduplicate. Callers do that, as the Postgres parser does.

Known limits, which are current behavior and not changed by this document:

- The function-call check builds its positions with a new `Regex` on every call, which is not cached.
- Only the operators listed in step 1 are recognized. A predicate that compares with another symbol
  (for example `@>` or `->>`) yields no column.
- The unary-minus case is deliberately ignored, so `- col = 1` is still reported.

## Constants and conventions

Each parser keeps its literal strings in a sibling `*Constants` class, as the repository conventions
require. These classes are `internal`. The SQL Server constants include the XML names and the
`EQUALITY` and `INEQUALITY` usage tokens, along with the two size limits and the failure message
formats. The Postgres constants hold the JSON property names. The factory extensions constants hold
the `ParseWith` error message.

Parsers never touch `Console` or `File`. They read only the string they are given, which keeps tests
able to feed inline plans.

## Adding a dialect

1. Implement `IPlanParser`. Keep `CanParse` cheap and make it distinguish the new format from the
   existing ones. Throw `PlanParseException` for bad input.
2. Put the parser's literal strings in a `*Constants` class.
3. Register it. Either pass it to `new PlanParserFactory(IEnumerable<IPlanParser>)` with the existing
   parsers, or add it to the default constructor's list.
4. Add fixtures under `tests/SqlIndexAdvisor.Tests/Fixtures/<Dialect>/` and tests named
   `<Subject>Tests`.

Adding a parser does not change how rules run. Rules compare `PlanNode.RelativeCost`, never absolute
cost, because SQL Server and PostgreSQL costs are not comparable. A new parser must therefore fill
`RelativeCost` on a scale that is consistent within one statement.

## Related documents

- [ExecutionPlan.md](ExecutionPlan.md): the model these parsers produce.
- [recommendation-engine.md](recommendation-engine.md): how the parsed plans are analyzed.
- [rules.md](rules.md): the rules that consume predicate and output columns.
- [ARCHITECTURE.md](ARCHITECTURE.md): design rationale for the model and engine.
