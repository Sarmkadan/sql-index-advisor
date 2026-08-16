using System.Text.Json;
using System.Threading;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Parses the output of EXPLAIN (FORMAT JSON) / EXPLAIN (ANALYZE, FORMAT JSON)
/// from PostgreSQL. Postgres does not emit missing-index hints, so all the
/// recommendations for PG come from the rules walking Seq Scan nodes with filters.
/// </summary>
public sealed class PostgresJsonPlanParser : IPlanParser
{
    // --------------------------------------------------------------------
    // Limits to protect against untrusted input that could otherwise cause
    // excessive memory usage or stack overflow.
    // --------------------------------------------------------------------
    private const int MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MiB
    private const int MaxNestingDepth = 1_000; // reasonable depth for a plan

    /// <summary>
    /// Determines whether the supplied content looks like a PostgreSQL JSON plan.
    /// </summary>
    /// <param name="content">The raw plan text.</param>
    /// <returns>True if the content appears to be a PostgreSQL JSON plan; otherwise false.</returns>
    public bool CanParse(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        var trimmed = content.TrimStart();
        if (trimmed.Length == 0 || (trimmed[0] != '[' && trimmed[0] != '{'))
            return false;
        return trimmed.Contains("\"Node Type\"", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("\"Plan\"", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses a PostgreSQL JSON execution plan into an <see cref="ExecutionPlan"/>.
    /// </summary>
    /// <param name="content">The JSON plan text.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>An <see cref="ExecutionPlan"/> representing the parsed plan.</returns>
    /// <exception cref="PlanParseException">
    /// Thrown when the input exceeds size or nesting limits, or when the JSON is malformed.
    /// </exception>
    public ExecutionPlan Parse(string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        if (content.Length > MaxFileSizeBytes)
            throw new PlanParseException(
                $"Plan file size exceeds the allowed limit of {MaxFileSizeBytes} bytes.");

        JsonDocument doc;
        try
        {
            // Use JsonDocument.ParseOptions to be more lenient with malformed JSON
            // but still throw on truly invalid input
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };
            doc = JsonDocument.Parse(content, options);
        }
        catch (JsonException ex)
        {
            throw new PlanParseException("Content is not valid JSON.", ex);
        }

        using (doc)
        {
            var root = doc.RootElement;
            // EXPLAIN JSON is normally an array with a single object holding "Plan".
            if (root.ValueKind == JsonValueKind.Array)
            {
                if (root.GetArrayLength() == 0)
                    throw new PlanParseException("Empty plan array.");
                root = root[0];
            }

            if (!root.TryGetProperty("Plan", out var planRoot))
                throw new PlanParseException("No 'Plan' property in JSON plan.");

            var totalCost = ReadDouble(planRoot, "Total Cost");
            var nodes = new List<PlanNode>();
            Walk(planRoot, null, totalCost, nodes, 0, cancellationToken);

            return new ExecutionPlan
            {
                Dialect = PlanDialect.Postgres,
                EstimatedTotalCost = totalCost,
                Nodes = nodes
            };
        }
    }

    private static void Walk(JsonElement el, PlanNode? parent, double totalCost,
        List<PlanNode> sink, int currentDepth, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (currentDepth > MaxNestingDepth)
            throw new PlanParseException(
                $"Plan nesting depth exceeds the allowed limit of {MaxNestingDepth} levels.");

        var nodeType = ReadString(el, "Node Type") ?? "Unknown";
        var nodeCost = ReadDouble(el, "Total Cost");

        var node = new PlanNode
        {
            Operator = nodeType,
            TableName = ReadString(el, "Relation Name"),
            IndexName = ReadString(el, "Index Name"),
            EstimatedRows = ReadDouble(el, "Plan Rows"),
            EstimatedRowsRead = ReadDouble(el, "Plan Rows"),
            RelativeCost = totalCost > 0 ? nodeCost / totalCost : 0,
            PredicateColumns = ExtractFilterColumns(el),
            OutputColumns = ReadStringArray(el, "Output"),
            Parent = parent
        };
        sink.Add(node);

        if (el.TryGetProperty("Plans", out var children) && children.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in children.EnumerateArray())
                Walk(child, node, totalCost, sink, currentDepth + 1, cancellationToken);
        }
    }

    /// <summary>
    /// Pulls column names out of the Filter / Index Cond text. Postgres gives us
    /// the raw expression string, e.g. "(status = 'open'::text)". We do a light
    /// tokenization: grab identifiers that sit immediately left of a comparison op.
    /// </summary>
    private static List<string> ExtractFilterColumns(JsonElement el)
    {
        var cols = new List<string>();
        foreach (var key in new[] { "Filter", "Index Cond", "Recheck Cond", "Hash Cond" })
        {
            var expr = ReadString(el, key);
            if (string.IsNullOrEmpty(expr)) continue;
            foreach (var c in PredicateColumnScanner.Scan(expr, true))
                if (!cols.Contains(c)) cols.Add(c);
        }
        return cols;
    }

    private static string? ReadString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static double ReadDouble(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0;

    private static List<string> ReadStringArray(JsonElement el, string prop)
    {
        var list = new List<string>();
        if (el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Array)
            foreach (var item in v.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String)
                    list.Add(item.GetString()!);
        return list;
    }
}
