using System.Text.Json;
using System.Threading;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Parses PostgreSQL execution plans produced by <c>EXPLAIN (FORMAT JSON)</c> or
/// <c>EXPLAIN (ANALYZE, FORMAT JSON)</c>.
/// </summary>
/// <remarks>
/// PostgreSQL plans do not contain missing-index hints. Index recommendations are
/// derived by inspecting sequential-scan nodes and their filter predicates.
/// </remarks>
public sealed class PostgresJsonPlanParser : IPlanParser
{
    // --------------------------------------------------------------------
    // Limits to protect against untrusted input that could otherwise cause
    // excessive memory usage or stack overflow.
    // --------------------------------------------------------------------
    private const int MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MiB
    private const int MaxNestingDepth = 1_000; // reasonable depth for a plan

    // JSON property names as constants to avoid magic strings
    private const string NodeTypePropertyName = "Node Type";
    private const string PlanPropertyName = "Plan";
    private const string TotalCostPropertyName = "Total Cost";
    private const string RelationNamePropertyName = "Relation Name";
    private const string IndexNamePropertyName = "Index Name";
    private const string PlanRowsPropertyName = "Plan Rows";
    private const string OutputPropertyName = "Output";
    private const string PlansPropertyName = "Plans";
    private const string FilterPropertyName = "Filter";
    private const string IndexCondPropertyName = "Index Cond";
    private const string RecheckCondPropertyName = "Recheck Cond";
    private const string HashCondPropertyName = "Hash Cond";

    /// <summary>
    /// Determines whether the supplied text appears to contain a PostgreSQL JSON execution plan.
    /// </summary>
    /// <param name="content">The plan text to inspect.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="content"/> resembles a PostgreSQL JSON plan;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="content"/> is <see langword="null"/> or empty.
    /// </exception>
    public bool CanParse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (content.Length == 0)
            throw new ArgumentException("The string cannot be empty.", nameof(content));

        var trimmed = content.TrimStart();
        if (trimmed.Length == 0 || (trimmed[0] != '[' && trimmed[0] != '{'))
            return false;
        return trimmed.Contains($"\"{NodeTypePropertyName}\"", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains($"\"{PlanPropertyName}\"", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses a PostgreSQL JSON execution plan into an <see cref="ExecutionPlan"/>.
    /// </summary>
    /// <param name="content">The PostgreSQL JSON plan text to parse.</param>
    /// <param name="cancellationToken">A token that can cancel traversal of the execution plan.</param>
    /// <returns>The execution plan represented by <paramref name="content"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="content"/> is <see langword="null"/> or empty.
    /// </exception>
    /// <exception cref="PlanParseException">
    /// The input exceeds the supported size or nesting limits, contains malformed JSON,
    /// or does not contain a PostgreSQL <c>Plan</c> object.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// <paramref name="cancellationToken"/> is canceled while traversing the plan.
    /// </exception>
    public ExecutionPlan Parse(string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (content.Length == 0)
            throw new ArgumentException("The string cannot be empty.", nameof(content));

        var contentSizeBytes = System.Text.Encoding.UTF8.GetByteCount(content);
        if (contentSizeBytes > MaxFileSizeBytes)
            throw new PlanParseException(
                $"Plan file size of {contentSizeBytes} bytes exceeds the allowed limit of {MaxFileSizeBytes} bytes.");

        JsonDocument doc;
        try
        {
            // Use JsonDocument.ParseOptions to be more lenient with malformed JSON
            // but still throw on truly invalid input
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
                MaxDepth = MaxNestingDepth
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

            if (!root.TryGetProperty(PlanPropertyName, out var planRoot))
                throw new PlanParseException($"No '{PlanPropertyName}' property in JSON plan.");

            var totalCost = ReadDouble(planRoot, TotalCostPropertyName);
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

        var nodeType = ReadString(el, NodeTypePropertyName) ?? "Unknown";
        var nodeCost = ReadDouble(el, TotalCostPropertyName);

        var node = new PlanNode
        {
            Operator = nodeType,
            TableName = ReadString(el, RelationNamePropertyName),
            IndexName = ReadString(el, IndexNamePropertyName),
            EstimatedRows = ReadDouble(el, PlanRowsPropertyName),
            EstimatedRowsRead = ReadDouble(el, PlanRowsPropertyName),
            RelativeCost = totalCost > 0 ? nodeCost / totalCost : 0,
            PredicateColumns = ExtractFilterColumns(el),
            OutputColumns = ReadStringArray(el, OutputPropertyName),
            Parent = parent
        };
        sink.Add(node);

        if (el.TryGetProperty(PlansPropertyName, out var children) && children.ValueKind == JsonValueKind.Array)
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
        foreach (var key in new[] { FilterPropertyName, IndexCondPropertyName, RecheckCondPropertyName, HashCondPropertyName })
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
