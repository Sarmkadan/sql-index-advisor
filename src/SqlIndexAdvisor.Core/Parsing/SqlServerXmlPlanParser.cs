using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Parses SQL Server showplan XML (the shape you get from SET STATISTICS XML ON
/// or "Save Execution Plan As..."). The showplan namespace is ignored - we match
/// on local element names so this survives version bumps of the schema URI.
/// </summary>
public sealed class SqlServerXmlPlanParser : IPlanParser
{
    /// <summary>
    /// Determines whether the supplied content appears to be a SQL Server showplan XML document.
    /// </summary>
    /// <param name="content">The plan content to inspect.</param>
    /// <returns><see langword="true"/> if the content appears to contain a SQL Server execution plan; otherwise, <see langword="false"/>.</returns>
    public bool CanParse(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        var trimmed = content.TrimStart();
        if (!trimmed.StartsWith('<')) return false;
        return trimmed.Contains(SqlServerXmlPlanParserConstants.ShowPlanXmlMarker, StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains(SqlServerXmlPlanParserConstants.StmtSimpleElement, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses SQL Server showplan XML into an execution plan.
    /// </summary>
    /// <param name="content">The SQL Server showplan XML content to parse.</param>
    /// <param name="cancellationToken">A token that can be used to cancel parsing.</param>
    /// <returns>The execution plan represented by the supplied XML.</returns>
    public ExecutionPlan Parse(string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        XDocument doc;
        try
        {
            // Harden against XXE and large documents
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = SqlServerXmlPlanParserConstants.MaxCharactersInDocument,
                MaxCharactersFromEntities = SqlServerXmlPlanParserConstants.MaxCharactersFromEntities
            };

            using var reader = XmlReader.Create(new StringReader(content), settings);
            doc = XDocument.Load(reader);
        }
        catch (XmlException ex)
        {
            // Provide context about the file type to help users diagnose issues
            throw new PlanParseException(
                string.Format(SqlServerXmlPlanParserConstants.ParseFailedMessageFormat, ex.LineNumber, ex.LinePosition), ex);
        }
        catch (Exception ex) when (ex is not PlanParseException)
        {
            throw new PlanParseException(SqlServerXmlPlanParserConstants.NotWellFormedXmlMessage, ex);
        }

        var stmt = Descendants(doc.Root, SqlServerXmlPlanParserConstants.StmtSimpleElement).FirstOrDefault();
        var statementText = stmt?.Attribute(SqlServerXmlPlanParserConstants.StatementTextAttribute)?.Value ?? string.Empty;
        var totalCost = ParseDouble(stmt?.Attribute(SqlServerXmlPlanParserConstants.StatementSubTreeCostAttribute)?.Value);

        var nodes = new List<PlanNode>();
        var byElement = new Dictionary<XElement, PlanNode>();
        var relOps = Descendants(doc.Root, SqlServerXmlPlanParserConstants.RelOpElement).ToList();

        foreach (var relOp in relOps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var estRows = ParseDouble(relOp.Attribute(SqlServerXmlPlanParserConstants.EstimateRowsAttribute)?.Value);
            var node = new PlanNode
            {
                Operator = relOp.Attribute(SqlServerXmlPlanParserConstants.PhysicalOpAttribute)?.Value ?? relOp.Attribute(SqlServerXmlPlanParserConstants.LogicalOpAttribute)?.Value ?? SqlServerXmlPlanParserConstants.UnknownOperator,
                EstimatedRows = estRows,
                EstimatedRowsRead = estRows,
                RelativeCost = totalCost > 0
                    ? ParseDouble(relOp.Attribute(SqlServerXmlPlanParserConstants.EstimatedTotalSubtreeCostAttribute)?.Value) / totalCost
                    : 0,
                TableName = FindObjectName(relOp),
                IndexName = FindIndexName(relOp),
                PredicateColumns = FindPredicateColumns(relOp),
                OutputColumns = FindOutputColumns(relOp)
            };
            nodes.Add(node);
            byElement[relOp] = node;
        }

        // Wire parent pointers: the parent of a RelOp is its nearest ancestor RelOp.
        // Rules that reason about tree shape (key lookups, join inner sides) need this.
        foreach (var (relOp, node) in byElement)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var parentEl = relOp.Ancestors().FirstOrDefault(a => a.Name.LocalName == SqlServerXmlPlanParserConstants.RelOpElement);
            if (parentEl is not null && byElement.TryGetValue(parentEl, out var parentNode))
                node.Parent = parentNode;
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
        ArgumentNullException.ThrowIfNull(root);

        var result = new List<EngineMissingIndex>();
        foreach (var group in Descendants(root, SqlServerXmlPlanParserConstants.MissingIndexGroupElement))
        {
            var impact = ParseDouble(group.Attribute(SqlServerXmlPlanParserConstants.ImpactAttribute)?.Value);
            foreach (var idx in Descendants(group, SqlServerXmlPlanParserConstants.MissingIndexElement))
            {
                var bareTable = idx.Attribute(SqlServerXmlPlanParserConstants.TableAttribute)?.Value?.Trim(SqlServerXmlPlanParserConstants.BracketOpen[0], SqlServerXmlPlanParserConstants.BracketClose[0]) ?? string.Empty;
                var schema = idx.Attribute(SqlServerXmlPlanParserConstants.SchemaAttribute)?.Value?.Trim(SqlServerXmlPlanParserConstants.BracketOpen[0], SqlServerXmlPlanParserConstants.BracketClose[0]);
                var table = string.IsNullOrEmpty(schema) ? bareTable : $"{schema}{SqlServerXmlPlanParserConstants.SchemaTableSeparator}{bareTable}";
                var eq = new List<string>();
                var ineq = new List<string>();
                var incl = new List<string>();
                foreach (var col in Descendants(idx, SqlServerXmlPlanParserConstants.ColumnGroupElement))
                {
                    var usage = col.Attribute(SqlServerXmlPlanParserConstants.UsageAttribute)?.Value ?? string.Empty;
                    var names = Descendants(col, SqlServerXmlPlanParserConstants.ColumnElement)
                        .Select(c => c.Attribute(SqlServerXmlPlanParserConstants.NameAttribute)?.Value?.Trim(SqlServerXmlPlanParserConstants.BracketOpen[0], SqlServerXmlPlanParserConstants.BracketClose[0]) ?? string.Empty)
                        .Where(n => n.Length > 0);
                    var target = usage switch
                    {
                        SqlServerXmlPlanParserConstants.EqualityUsage => eq,
                        SqlServerXmlPlanParserConstants.InequalityUsage => ineq,
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
        ArgumentNullException.ThrowIfNull(relOp);

        var obj = Descendants(relOp, SqlServerXmlPlanParserConstants.ObjectElement).FirstOrDefault();
        if (obj is null) return null;
        var table = obj.Attribute(SqlServerXmlPlanParserConstants.TableAttribute)?.Value?.Trim(SqlServerXmlPlanParserConstants.BracketOpen[0], SqlServerXmlPlanParserConstants.BracketClose[0]);
        var schema = obj.Attribute(SqlServerXmlPlanParserConstants.SchemaAttribute)?.Value?.Trim(SqlServerXmlPlanParserConstants.BracketOpen[0], SqlServerXmlPlanParserConstants.BracketClose[0]);
        if (string.IsNullOrEmpty(table)) return null;
        return string.IsNullOrEmpty(schema) ? table : $"{schema}{SqlServerXmlPlanParserConstants.SchemaTableSeparator}{table}";
    }

    private static string? FindIndexName(XElement relOp)
    {
        ArgumentNullException.ThrowIfNull(relOp);

        var obj = Descendants(relOp, SqlServerXmlPlanParserConstants.ObjectElement).FirstOrDefault();
        return obj?.Attribute(SqlServerXmlPlanParserConstants.IndexAttribute)?.Value?.Trim(SqlServerXmlPlanParserConstants.BracketOpen[0], SqlServerXmlPlanParserConstants.BracketClose[0]);
    }

    private static List<string> FindPredicateColumns(XElement relOp)
    {
        ArgumentNullException.ThrowIfNull(relOp);

        // Only look at predicates directly under this RelOp, not nested RelOps.
        var cols = new List<string>();
        foreach (var pred in DirectDescendantsBeforeNestedRelOp(relOp, SqlServerXmlPlanParserConstants.PredicateElement))
        {
            foreach (var c in Descendants(pred, SqlServerXmlPlanParserConstants.ColumnReferenceElement))
            {
                var name = c.Attribute(SqlServerXmlPlanParserConstants.ColumnAttribute)?.Value?.Trim(SqlServerXmlPlanParserConstants.BracketOpen[0], SqlServerXmlPlanParserConstants.BracketClose[0]);
                if (!string.IsNullOrEmpty(name) && !cols.Contains(name))
                    cols.Add(name);
            }
        }
        return cols;
    }

    private static List<string> FindOutputColumns(XElement relOp)
    {
        ArgumentNullException.ThrowIfNull(relOp);

        var cols = new List<string>();
        var outputList = DirectDescendantsBeforeNestedRelOp(relOp, SqlServerXmlPlanParserConstants.OutputListElement).FirstOrDefault();
        if (outputList is null) return cols;
        foreach (var c in Descendants(outputList, SqlServerXmlPlanParserConstants.ColumnReferenceElement))
        {
            var name = c.Attribute(SqlServerXmlPlanParserConstants.ColumnAttribute)?.Value?.Trim(SqlServerXmlPlanParserConstants.BracketOpen[0], SqlServerXmlPlanParserConstants.BracketClose[0]);
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
        ArgumentNullException.ThrowIfNull(relOp);

        foreach (var el in relOp.Elements())
        {
            if (el.Name.LocalName == SqlServerXmlPlanParserConstants.RelOpElement) continue; // skip the child operator subtree
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
