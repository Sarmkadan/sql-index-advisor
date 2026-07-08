using System.Text.Json;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Parses the output of EXPLAIN (FORMAT JSON) / EXPLAIN (ANALYZE, FORMAT JSON)
/// from PostgreSQL. Postgres does not emit missing-index hints, so all the
/// recommendations for PG come from the rules walking Seq Scan nodes with filters.
/// </summary>
public sealed class PostgresJsonPlanParser : IPlanParser
{
    public bool CanParse(string content)
    {
        var trimmed = content.TrimStart();
        if (trimmed.Length == 0 || (trimmed[0] != '[' && trimmed[0] != '{')) return false;
        return trimmed.Contains("\"Node Type\"", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("\"Plan\"", StringComparison.OrdinalIgnoreCase);
    }

    public ExecutionPlan Parse(string content)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(content);
        }
        catch (Exception ex)
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
            Walk(planRoot, null, totalCost, nodes);

            return new ExecutionPlan
            {
                Dialect = PlanDialect.Postgres,
                EstimatedTotalCost = totalCost,
                Nodes = nodes
            };
        }
    }

    private static void Walk(JsonElement el, PlanNode? parent, double totalCost, List<PlanNode> sink)
    {
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
                Walk(child, node, totalCost, sink);
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
            foreach (var c in PredicateColumnScanner.Scan(expr))
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
