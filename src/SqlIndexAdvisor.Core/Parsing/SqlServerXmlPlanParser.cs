using System.Xml.Linq;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Parses SQL Server showplan XML (the shape you get from SET STATISTICS XML ON
/// or "Save Execution Plan As..."). The showplan namespace is ignored - we match
/// on local element names so this survives version bumps of the schema URI.
/// </summary>
public sealed class SqlServerXmlPlanParser : IPlanParser
{
    public bool CanParse(string content)
    {
        var trimmed = content.TrimStart();
        if (!trimmed.StartsWith('<')) return false;
        return trimmed.Contains("ShowPlanXML", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("StmtSimple", StringComparison.OrdinalIgnoreCase);
    }

    public ExecutionPlan Parse(string content)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(content);
        }
        catch (Exception ex)
        {
            throw new PlanParseException("Content is not well-formed XML.", ex);
        }

        var stmt = Descendants(doc.Root, "StmtSimple").FirstOrDefault();
        var statementText = stmt?.Attribute("StatementText")?.Value ?? string.Empty;
        var totalCost = ParseDouble(stmt?.Attribute("StatementSubTreeCost")?.Value);

        var nodes = new List<PlanNode>();
        var relOps = Descendants(doc.Root, "RelOp").ToList();
        foreach (var relOp in relOps)
        {
            var estRows = ParseDouble(relOp.Attribute("EstimateRows")?.Value);
            var node = new PlanNode
            {
                Operator = relOp.Attribute("PhysicalOp")?.Value ?? relOp.Attribute("LogicalOp")?.Value ?? "Unknown",
                EstimatedRows = estRows,
                EstimatedRowsRead = estRows,
                RelativeCost = totalCost > 0
                    ? ParseDouble(relOp.Attribute("EstimatedTotalSubtreeCost")?.Value) / totalCost
                    : 0,
                TableName = FindObjectName(relOp),
                IndexName = FindIndexName(relOp),
                PredicateColumns = FindPredicateColumns(relOp),
                OutputColumns = FindOutputColumns(relOp)
            };
            nodes.Add(node);
        }

        var engineHints = ParseMissingIndexes(doc.Root);

        return new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            StatementText = statementText,
            EstimatedTotalCost = totalCost,
            Nodes = nodes,
            EngineMissingIndexes = engineHints
        };
    }

    private static List<EngineMissingIndex> ParseMissingIndexes(XElement? root)
    {
        var result = new List<EngineMissingIndex>();
        foreach (var group in Descendants(root, "MissingIndexGroup"))
        {
            var impact = ParseDouble(group.Attribute("Impact")?.Value);
            foreach (var idx in Descendants(group, "MissingIndex"))
            {
                var bareTable = idx.Attribute("Table")?.Value?.Trim('[', ']') ?? string.Empty;
                var schema = idx.Attribute("Schema")?.Value?.Trim('[', ']');
                var table = string.IsNullOrEmpty(schema) ? bareTable : $"{schema}.{bareTable}";
                var eq = new List<string>();
                var ineq = new List<string>();
                var incl = new List<string>();
                foreach (var col in Descendants(idx, "ColumnGroup"))
                {
                    var usage = col.Attribute("Usage")?.Value ?? string.Empty;
                    var names = Descendants(col, "Column")
                        .Select(c => c.Attribute("Name")?.Value?.Trim('[', ']') ?? string.Empty)
                        .Where(n => n.Length > 0);
                    var target = usage switch
                    {
                        "EQUALITY" => eq,
                        "INEQUALITY" => ineq,
                        _ => incl
                    };
                    target.AddRange(names);
                }
                result.Add(new EngineMissingIndex
                {
                    Table = table,
                    ImpactPercent = impact,
                    EqualityColumns = eq,
                    InequalityColumns = ineq,
                    IncludeColumns = incl
                });
            }
        }
        return result;
    }

    private static string? FindObjectName(XElement relOp)
    {
        var obj = Descendants(relOp, "Object").FirstOrDefault();
        if (obj is null) return null;
        var table = obj.Attribute("Table")?.Value?.Trim('[', ']');
        var schema = obj.Attribute("Schema")?.Value?.Trim('[', ']');
        if (string.IsNullOrEmpty(table)) return null;
        return string.IsNullOrEmpty(schema) ? table : $"{schema}.{table}";
    }

    private static string? FindIndexName(XElement relOp)
    {
        var obj = Descendants(relOp, "Object").FirstOrDefault();
        return obj?.Attribute("Index")?.Value?.Trim('[', ']');
    }

    private static List<string> FindPredicateColumns(XElement relOp)
    {
        // Only look at predicates directly under this RelOp, not nested RelOps.
        var cols = new List<string>();
        foreach (var pred in DirectDescendantsBeforeNestedRelOp(relOp, "Predicate"))
        {
            foreach (var c in Descendants(pred, "ColumnReference"))
            {
                var name = c.Attribute("Column")?.Value?.Trim('[', ']');
                if (!string.IsNullOrEmpty(name) && !cols.Contains(name))
                    cols.Add(name);
            }
        }
        return cols;
    }

    private static List<string> FindOutputColumns(XElement relOp)
    {
        var cols = new List<string>();
        var outputList = DirectDescendantsBeforeNestedRelOp(relOp, "OutputList").FirstOrDefault();
        if (outputList is null) return cols;
        foreach (var c in Descendants(outputList, "ColumnReference"))
        {
            var name = c.Attribute("Column")?.Value?.Trim('[', ']');
            if (!string.IsNullOrEmpty(name) && !cols.Contains(name))
                cols.Add(name);
        }
        return cols;
    }

    private static IEnumerable<XElement> Descendants(XElement? root, string localName) =>
        root is null
            ? Enumerable.Empty<XElement>()
            : root.Descendants().Where(e => e.Name.LocalName == localName);

    // Predicates/outputs of child RelOps should not leak up into the parent.
    private static IEnumerable<XElement> DirectDescendantsBeforeNestedRelOp(XElement relOp, string localName)
    {
        foreach (var el in relOp.Elements())
        {
            if (el.Name.LocalName == "RelOp") continue; // skip the child operator subtree
            if (el.Name.LocalName == localName) yield return el;
            foreach (var nested in DirectDescendantsBeforeNestedRelOp(el, localName))
                yield return nested;
        }
    }

    private static double ParseDouble(string? raw) =>
        double.TryParse(raw, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var v)
            ? v
            : 0;
}
