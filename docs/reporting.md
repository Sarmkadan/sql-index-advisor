# Reporting

The `SqlIndexAdvisor.Core.Reporting` namespace turns an `ExecutionPlan` and its `IndexRecommendation`s into a string in one of several output formats. This document describes the `IReportRenderer` interface, the renderer classes that exist today, how the CLI selects among them, and what their output looks like.

Sample output below was captured by running the CLI (`--format text|json|html|csv`) and a small scratch harness (Markdown and grouped multi-statement output) against the repository's `samples/sqlserver_orders_scan.sqlplan`. Column values and reasons are the engine's current output and may change as rules evolve.

## Overview

```
plan text -> PlanParserFactory -> StatementReport[] -> StatementReportRenderer -> stdout
                                                        |
                                                        +-> ReportRenderer        (text, json)
                                                        +-> HtmlReportRenderer    (html)
                                                        +-> CsvReportRenderer     (csv)
                                                        +-> MarkdownReportRenderer (markdown)
```

Each renderer takes the same two inputs: the plan (for dialect, total cost and operator count, and for `CREATE INDEX` dialect rules) and the list of recommendations. All renderers are pure functions of their inputs; they do no I/O. Writing to stdout is done by the CLI (`src/SqlIndexAdvisor.Cli/Program.cs`).

## `IReportRenderer`

```csharp
public interface IReportRenderer
{
    string RenderText(ExecutionPlan plan, IReadOnlyList<IndexRecommendation> recs);
    string RenderJson(ExecutionPlan plan, IReadOnlyList<IndexRecommendation> recs, string schemaVersion = "1.0");
}
```

`IReportRenderer` declares the two original renderings, text and JSON, as instance methods. **Nothing in the repository implements it.** The concrete renderers are `static` classes, and the CLI does not resolve a renderer through this interface. The interface should be treated as a declared contract for a future instance-based renderer, not as the entry point to rendering today. Use the static classes below.

## Renderers

### `ReportRenderer` (text and JSON)

File: `src/SqlIndexAdvisor.Core/Reporting/ReportRenderer.cs`

| Method | Format | Notes |
| --- | --- | --- |
| `RenderText(plan, recs)` | plain text | Human-readable summary; one block per recommendation. |
| `RenderJson(plan, recs, schemaVersion = "1.0")` | JSON, indented | Machine-readable; stable field names listed below. |

Both methods throw `ArgumentNullException` when an argument is null. `RenderJson` also rejects a null `schemaVersion`.

**Text sample** (`--format text`, default):

```text
Dialect : SqlServer
Statement cost: 4.812
Operators : 1

1 recommendation(s):

[1] High confidence | ~82.5% estimated impact
 CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (OrderId, CustomerId, Total);
 - Clustered Index Scan on dbo.Orders carries a filter on (Status, CreatedAt) and is ~100% of statement cost.
 - Row counts are not in the plan, so the table-size and selectivity thresholds were not applied.
 - Optimizer reported a missing index with 82.5% estimated impact.

Impact figures are rough heuristics, not measured gains. Validate before applying.
```

When there are no recommendations, the text report prints the header lines and then `No index recommendations. The plan looks fine or lacks the detail needed to suggest one.` and stops.

**JSON sample** (`--format json`):

```json
{
  "schemaVersion": "1.0",
  "dialect": "SqlServer",
  "estimatedTotalCost": 4.812,
  "operatorCount": 1,
  "recommendations": [
    {
      "id": "rec-1",
      "table": "dbo.Orders",
      "keyColumns": [
        "Status",
        "CreatedAt"
      ],
      "includeColumns": [
        "OrderId",
        "CustomerId",
        "Total"
      ],
      "estimatedImpactPercent": 82.4711,
      "confidence": "High",
      "reasons": [
        "Clustered Index Scan on dbo.Orders carries a filter on (Status, CreatedAt) and is ~100% of statement cost.",
        "Row counts are not in the plan, so the table-size and selectivity thresholds were not applied.",
        "Optimizer reported a missing index with 82.5% estimated impact."
      ],
      "createStatement": "CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (OrderId, CustomerId, Total);",
      "ddl": "CREATE NONCLUSTERED INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (OrderId, CustomerId, Total) WITH (ONLINE = ON);",
      "originatingRule": "engine-hint"
    }
  ]
}
```

JSON fields:

| Field | Source |
| --- | --- |
| `schemaVersion` | argument, default `"1.0"` |
| `dialect` | `plan.Dialect` |
| `estimatedTotalCost` | `plan.EstimatedTotalCost`, unrounded |
| `operatorCount` | `plan.Nodes.Count` |
| `recommendations[].id` | `rec-N`, one-based position in the list |
| `recommendations[].table`, `keyColumns`, `includeColumns` | the recommendation |
| `recommendations[].estimatedImpactPercent` | unrounded double |
| `recommendations[].confidence` | enum name as a string |
| `recommendations[].reasons` | array of strings |
| `recommendations[].createStatement` | `ToCreateStatement(dialect)`, the portable one-line form |
| `recommendations[].ddl` | `DdlRenderer.RenderCreateIndex`, the dialect-specific DDL |
| `recommendations[].originatingRule` | `recommendation.Rule` |

Note that `createStatement` and `ddl` can differ: `ddl` carries dialect options such as `ONLINE = ON`, while `createStatement` does not.

### `HtmlReportRenderer`

File: `src/SqlIndexAdvisor.Core/Reporting/HtmlReportRenderer.cs`

- `RenderHtml(ExecutionPlan, IReadOnlyList<IndexRecommendation>)` renders one statement.
- `RenderHtml(IReadOnlyList<StatementReport>)` renders one or more statements as one document. With more than one statement, each section gets an `<h2>Statement N</h2>` heading.

The output is a complete, self-contained HTML5 document with an inline `<style>` block. Table rows get a CSS class from confidence: `high`, `medium`, `low`, or `unknown`. Create statements and reasons are HTML-encoded.

**Sample** (`--format html`, body only, abridged):

```html
<body>
<h1>SQL Index Advisor Report</h1>
<div class="summary">
<p><strong>Dialect:</strong> SqlServer</p>
<p><strong>Statement cost:</strong> 4.812</p>
<p><strong>Operators:</strong> 1</p>
</div>
<h2>1 recommendation(s)</h2>
<table>
<thead>
<tr>
<th>#</th>
<th>Confidence</th>
<th>Impact %</th>
<th>Create Statement</th>
<th>Reasons</th>
</tr>
</thead>
<tbody>
<tr class="high">
<td>1</td>
<td>High</td>
<td>82.5%</td>
<td><code>CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (OrderId, CustomerId, Total);</code></td>
<td><ul class="reasons">
<li>Clustered Index Scan on dbo.Orders carries a filter on (Status, CreatedAt) and is ~100% of statement cost.</li>
<li>Row counts are not in the plan, so the table-size and selectivity thresholds were not applied.</li>
<li>Optimizer reported a missing index with 82.5% estimated impact.</li>
</ul></td>
</tr>
</tbody>
</table>
<p>Impact figures are rough heuristics, not measured gains. Validate before applying.</p>
</body>
```

With no recommendations, the section shows `<p>No index recommendations. ...</p>` in place of the table.

### `CsvReportRenderer`

File: `src/SqlIndexAdvisor.Core/Reporting/CsvReportRenderer.cs`

`RenderCsv(ExecutionPlan, IReadOnlyList<IndexRecommendation>)` writes an RFC 4180 document. Fields containing commas, quotes, CR or LF are quoted, and embedded quotes are doubled. The header is always present, even with zero rows.

Columns: `Confidence,ImpactPercent,CreateStatement,Reasons`. Reasons are joined with `"; "`.

**Sample** (`--format csv`):

```csv
Confidence,ImpactPercent,CreateStatement,Reasons
High,82.5,"CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (OrderId, CustomerId, Total);","Clustered Index Scan on dbo.Orders carries a filter on (Status, CreatedAt) and is ~100% of statement cost.; Row counts are not in the plan, so the table-size and selectivity thresholds were not applied.; Optimizer reported a missing index with 82.5% estimated impact."
```

`ImpactPercent` is formatted with `0.#` and the invariant culture, so the decimal separator is always `.`.

### `MarkdownReportRenderer`

File: `src/SqlIndexAdvisor.Core/Reporting/MarkdownReportRenderer.cs` (constants in `MarkdownReportRendererConstants.cs`)

`RenderMarkdown(ExecutionPlan, IReadOnlyList<IndexRecommendation>)` produces GitHub-flavored Markdown: a summary list, a recommendations table, and then one `### <table>` section per recommendation with a `sql` fenced block containing the dialect-specific DDL from `DdlRenderer`. Pipes in cell values are escaped as `\|`.

**Sample** (abridged):

````markdown
# SQL Index Advisor Report

- **Dialect:** SqlServer
- **Estimated total cost:** 4.812

## Recommendations

| Table | Key columns | Includes | Confidence | Estimated impact % | Reason |
| --- | --- | --- | --- | ---: | --- |
| dbo.Orders | Status, CreatedAt | OrderId, CustomerId, Total | High | 82.5 | Clustered Index Scan on dbo.Orders carries a filter on (Status, CreatedAt) and is ~100% of statement cost.; Row counts are not in the plan, so the table-size and selectivity thresholds were not applied.; Optimizer reported a missing index with 82.5% estimated impact. |

### dbo.Orders

```sql
CREATE NONCLUSTERED INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (OrderId, CustomerId, Total) WITH (ONLINE = ON);
```
````

The CLI does not currently accept `markdown`: `ArgsParser` validates `--format` against `text`, `json`, `html` and `csv` only, so `--format markdown` exits with an error. The renderer is reachable from code, and `Program.cs` also lists `markdown` in its usage text. See [Known gaps](#known-gaps).

### `DdlRenderer`

File: `src/SqlIndexAdvisor.Core/Reporting/DdlRenderer.cs`

Not a report format, but used by the JSON and Markdown renderers. `DdlRenderer.RenderCreateIndex(recommendation, dialect)` returns the dialect-specific `CREATE INDEX` DDL, which can include options such as `ONLINE = ON` that the portable `ToCreateStatement` form omits.

### `StatementReportRenderer` (multi-statement front end)

File: `src/SqlIndexAdvisor.Core/Reporting/StatementReportRenderer.cs`

This is the entry point the CLI uses. `Render(IReadOnlyList<StatementReport>, format, schemaVersion = "1.0")` takes a `StatementReport` per statement (`Index`, `Plan`, `Recommendations`) and dispatches on `format`:

- **One statement:** the output is exactly what the format's own renderer produces. Single-statement output is unchanged by this class.
- **Several statements:** output is grouped by statement, in input order.

| Format | Several statements |
| --- | --- |
| `text` (and any unrecognised format) | One `=== Statement N of M: <summary> ===` section per statement, each with the text report. |
| `json` | A JSON array with one report object per statement. |
| `html` | One document; each statement gets an `<h2>Statement N</h2>` section. |
| `csv` | One header row, then the data rows of every statement. |
| `markdown` | One `## Statement N of M: <summary>` section per statement. |

The statement summary is the statement text collapsed to one line and cut to 80 characters, ending with `...`.

**Multi-statement text sample** (the same plan supplied twice, for illustration):

```text
=== Statement 1 of 2: SELECT OrderId, CustomerId, Total FROM dbo.Orders WHERE Status = 'open' AND C... ===
Dialect : SqlServer
Statement cost: 4.812
Operators : 1

1 recommendation(s):

[1] High confidence | ~82.5% estimated impact
 CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt) INCLUDE (OrderId, CustomerId, Total);
 - Clustered Index Scan on dbo.Orders carries a filter on (Status, CreatedAt) and is ~100% of statement cost.
 - Row counts are not in the plan, so the table-size and selectivity thresholds were not applied.
 - Optimizer reported a missing index with 82.5% estimated impact.

Impact figures are rough heuristics, not measured gains. Validate before applying.

=== Statement 2 of 2: SELECT OrderId, CustomerId, Total FROM dbo.Orders WHERE Status = 'open' AND C... ===
...
```

**Multi-statement CSV sample:** one header, then one row per recommendation per statement:

```csv
Confidence,ImpactPercent,CreateStatement,Reasons
High,82.5,"CREATE INDEX ...","Clustered Index Scan ...; Row counts ...; Optimizer reported ..."
High,82.5,"CREATE INDEX ...","Clustered Index Scan ...; Row counts ...; Optimizer reported ..."
```

The CSV output has no statement column, so rows from different statements are not distinguishable in the file.

## Choosing a format

| Need | Format |
| --- | --- |
| Read in a terminal | `text` (default) |
| Feed another tool or script | `json` (versioned by `schemaVersion`) |
| Share or review in a browser | `html` |
| Spreadsheet or bulk import | `csv` (no per-statement column, no DDL) |
| Paste into a pull request or issue | `markdown` (library use only, see below) |

## Known gaps

- `IReportRenderer` is declared but not implemented. The static renderers above are the only rendering path.
- `markdown` is supported by `StatementReportRenderer` and `MarkdownReportRenderer`, but `ArgsParser` rejects `--format markdown`, so the CLI cannot produce it.
- `ImpactPercent` in CSV and the impact figures in text and HTML use `0.#` formatting; the CSV and JSON paths use the invariant culture, while text and HTML use the current culture. JSON keeps the unrounded value.

## Related

- [`docs/ARCHITECTURE.md`](ARCHITECTURE.md): pipeline and design rationale
- [`docs/IndexRecommendation.md`](IndexRecommendation.md): the model rendered by these classes
- [`docs/ExecutionPlan.md`](ExecutionPlan.md): the plan fields used in report headers
