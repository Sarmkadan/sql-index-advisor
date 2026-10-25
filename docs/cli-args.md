# Command-line arguments and JSON output

This document describes what `ArgsParser` (`src/SqlIndexAdvisor.Core/ArgsParsing/ArgsParser.cs`) accepts, how
`src/SqlIndexAdvisor.Cli/Program.cs` acts on the result, and how JSON is produced and read back. It describes the
current behavior of the code; where the code and its own help text disagree, that is called out.

## Synopsis

```text
sql-index-advisor <plan-file> [--format text|json|html|csv] [--min-impact <n>] [--fail-on-findings] [--fail-on <severity>]
sql-index-advisor - [--format text|json|html|csv] < input.json
sql-index-advisor --stdin [--format text|json|html|csv] [--fail-on-findings] [--fail-on <severity>]
```

Options may appear in any order. The plan source is one of: a positional path, `--plan <path>`, `-`, or `--stdin`.

## Options

| Option | Argument | Default | Meaning |
| --- | --- | --- | --- |
| `<plan-file>` (positional) | path | none | Plan file to read. Same as `--plan`. |
| `-` (positional) | none | none | Read the plan from standard input. |
| `--plan` | path | none | Explicit plan file. Requires a value. |
| `--stdin` | none | off | Read the plan from standard input. |
| `--format` | `text`, `json`, `html`, `csv` | `text` | Output format. The value is lowercased before it is checked. Requires a value. |
| `--min-impact` | number, 0 to 100 | `0` | Hide recommendations whose estimated impact percent is below this value. |
| `--fail-on-findings` | none | off | Exit with code 1 if any recommendation remains after `--min-impact` filtering. |
| `--fail-on` | `low`, `medium`, `high` | not set | Exit with code 1 if any recommendation has a confidence at or above this level. Case-insensitive. |
| `--sqlserver` | none | off | Asserts SQL Server input. See [Dialect flags](#dialect-flags). |
| `--postgres` | none | off | Asserts PostgreSQL input. See [Dialect flags](#dialect-flags). |
| `--version` | none | | Prints `sql-index-advisor version 1.0.0` and exits with code 0. |
| `-h`, `--help` | none | | Prints usage and exits with code 0. |

Constants for these names live in `ArgsParserConstants.cs`.

## Parsing rules

`ArgsParser.Parse(string[])` walks the arguments once, left to right. Validation failures are reported by throwing
`ArgumentException`. `Program` catches it, writes `error: <message>` to stderr, and exits with code 1.
`ParseResult.Error(...)` exists, but `Parse` never returns it.

Per-argument behavior:

- **No arguments.** Returns the usage text as a help result. `Program` prints it and exits with code 1.
- **`-h` / `--help`.** Returns the usage text immediately. `Program` exits with code 0.
- **`--version`.** Returns the version text immediately. `Program` exits with code 0.
- **Early return.** `--help` and `--version` return as soon as they are reached. Options that come after them
  are not validated. Options that come before them are: `--bogus --help` fails with `unknown option '--bogus'.`
- **Unknown option.** Any argument that starts with `-` and is not a known flag throws `unknown option '<arg>'.`
  The single `-` is the only exception; it is treated as the stdin path.
- **Value-taking options.** `--plan`, `--format`, `--min-impact`, and `--fail-on` each consume the next argument.
  If none is left, the parser throws `<option> requires a value.` The next argument is consumed even if it starts
  with `-`.
- **Repeated options.** `--format`, `--min-impact`, and `--fail-on` keep the last value. A second `--plan` or
  positional path throws `multiple plan sources specified`.
- **`--min-impact`.** Parsed with the invariant culture. Non-numeric input throws `--min-impact expects a number`.
  NaN, infinity, values below 0, and values above 100 throw `--min-impact must be between 0 and 100`.
- **`--fail-on`.** The value is lowercased and must be `low`, `medium`, or `high`. Anything else throws
  `--fail-on expects low, medium or high`.
- **`--format`.** Checked after the loop finishes. Anything other than `text`, `json`, `html`, or `csv` throws
  `--format must be 'text', 'json', 'html', or 'csv'`.

Cross-option checks, applied after the loop:

- `--sqlserver` together with `--postgres` throws `cannot specify both --sqlserver and --postgres.`
- `--stdin` together with a file path throws `cannot specify both --stdin and a file path.`
- With no plan source at all, `Program` throws `provide a plan file path or --stdin.`, which exits with code 1.

### Dialect flags

`--sqlserver` and `--postgres` are validated for mutual exclusion and recorded in the parser's activity trace.
They do not change parsing. `ParseResult` has no dialect field, and `Program` always calls
`new PlanParserFactory().Parse(content)`, which picks the parser by content. Passing `--postgres` to a SQL Server
plan still produces a SQL Server analysis.

### Plan source and path validation

Applies to the positional path and `--plan`. It does not apply to `-` or `--stdin`.

- A path segment equal to `..` throws `Path traversal sequences (..) are not allowed.`
- A path starting with `\\` or `//` throws `UNC paths are not allowed.`
- The path is resolved with `Path.GetFullPath`. The resolved path must start with the current directory
  (`Path.GetFullPath(".")`), compared case-insensitively. Otherwise the parser throws
  `Paths outside the current directory are not allowed.`

Known quirk: the containment check is a plain string-prefix test. A current directory of `/work/proj` also admits
`/work/proj-old/plan.json`. Keep this in mind if the check is ever tightened.

### Reading input

`ArgsParser.ReadFileWithEncoding(path)` is the one place input is read:

- `-` reads all of standard input through `Console.In`.
- A path that does not exist throws `FileNotFoundException` with `plan file not found: <path>`.
- Files are read as UTF-8 with BOM detection, so UTF-8, UTF-16, and UTF-32 BOMs are honored.

`Program` then requires non-whitespace content. Empty or whitespace-only input throws
`PlanParseException("plan content is empty.")`, which exits with code 2.

## Exit codes

| Code | When |
| --- | --- |
| `0` | Success. Also `--help`, `-h`, and `--version`. |
| `1` | Usage or I/O error (including the no-arguments usage case), or a failed `--fail-on-findings` or `--fail-on` check. |
| `2` | The plan could not be parsed (`PlanParseException`), including empty input. |

Failure checks run after the report is printed. Both checks may apply at once; either one returns 1.

- `--fail-on-findings` fails when the total recommendation count across all statements is greater than zero.
- `--fail-on <severity>` fails when some recommendation has `Confidence >= <severity>`. The order is
  `Low < Medium < High`.

`--min-impact` filtering runs before both checks, so filtered-out recommendations do not cause failures.

## Output formats

`Program` calls `StatementReportRenderer.Render`. A plan with one statement renders with that format's own renderer.
A plan with several statements is grouped:

- **text.** One `=== Statement N of M: <summary> ===` section per statement.
- **json.** A JSON array with one report object per statement, in input order.
- **html.** One section per statement.
- **csv.** One header row, then the rows of every statement.

### Documentation mismatch: `markdown`

`Program` and `StatementReportRenderer` both handle `markdown`, and `Program`'s own usage string lists it.
`ArgsParser` rejects it, and its usage text omits it. Through the CLI, `--format markdown` therefore fails with
`--format must be 'text', 'json', 'html', or 'csv'`. The markdown renderer is only reachable through the Core API.

## JSON output

### Report JSON (`--format json`)

Produced by `ReportRenderer.RenderJson(plan, recs, schemaVersion)`. It is indented, and property names are the
literal strings in the anonymous payload (already camelCase). Top-level keys:

| Key | Type | Notes |
| --- | --- | --- |
| `schemaVersion` | string | Always `"1.0"` from the CLI. |
| `dialect` | string | `SqlServer` or `Postgres` (the `PlanDialect` enum name). |
| `estimatedTotalCost` | number | Taken from the plan. |
| `operatorCount` | number | Number of plan nodes. |
| `recommendations` | array | See below. |

Each entry of `recommendations`:

| Key | Notes |
| --- | --- |
| `id` | `rec-N`, numbered by position within its statement. Not unique across statements. |
| `table` | Table name. |
| `keyColumns` | Array of strings. |
| `includeColumns` | Array of strings. |
| `estimatedImpactPercent` | Heuristic, not a measured gain. |
| `confidence` | `Low`, `Medium`, or `High` (enum name, not a number). |
| `reasons` | Array of human-readable strings. |
| `createStatement` | Output of `ToCreateStatement(dialect)`. |
| `ddl` | Output of `DdlRenderer.RenderCreateIndex(rec, dialect)`. |
| `originatingRule` | Name of the rule that produced the recommendation. |

With several statements, `--format json` prints `[` , one report object per statement separated by commas, then `]`.
Each object carries its own `schemaVersion`, and `id` restarts at `rec-1` in each.

### Recommendation JSON helpers

`IndexRecommendationJsonExtensionsJsonExtensions` (in `src/SqlIndexAdvisor.Core/Model/`) is a separate serializer for
a single `IndexRecommendation`. It is not used by the CLI, and no tests cover it. Its output differs from the
report JSON above.

- **Naming.** `JsonNamingPolicy.CamelCase` on the record's public properties. Keys are `table`, `keyColumns`,
  `includeColumns`, `estimatedImpactPercent`, `sourceNodeCost`, `confidence`, `rule`, `reasons`, `kind`, and
  `existingIndexName`. The rule key is `rule`, not `originatingRule`.
- **Enums.** Written as strings through `JsonStringEnumConverter`, for example `"High"` or `"CreateIndex"`.
- **No other options.** Reading does not set `PropertyNameCaseInsensitive`, so property names must match the
  camelCase form exactly.

Methods:

- `ToJson(this IndexRecommendation value, bool indented = false)` serializes the recommendation. `indented` controls
  the layout. A null `value` throws `ArgumentNullException`.
- `FromJson(string json)` deserializes. It throws `ArgumentException` for null, empty, or whitespace input, and
  `JsonException` for invalid JSON or a shape mismatch. The `required` members `table` and `keyColumns` must be present.
  The literal `null` returns `null`.
- `TryFromJson(string json, out IndexRecommendation? value)` returns `false` instead of throwing `JsonException`, and
  `false` for a literal `null`. It still throws `ArgumentException` for null, empty, or whitespace input.

## Related

- `src/SqlIndexAdvisor.Core/Reporting/ReportRenderer.cs`: report JSON.
- `src/SqlIndexAdvisor.Core/Reporting/StatementReportRenderer.cs`: multi-statement grouping.
- `tests/SqlIndexAdvisor.Tests/ArgsParserTests.cs`: parser tests.
