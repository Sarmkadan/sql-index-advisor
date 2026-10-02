using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Detects implicit conversions in predicate expressions that can prevent index usage.
/// Flags CONVERT_IMPLICIT operations in SQL Server and type-mismatch casts in Postgres.
/// Recommends aligning column/parameter types to enable index seeks.
/// </summary>
public sealed class ImplicitConversionRule : PlanNodeVisitorBase
{
    /// <summary>
    /// Gets the name of this rule.
    /// </summary>
    public override string Name => "implicit-conversion";

    /// <summary>
    /// Determines whether to visit a plan node. This rule does not visit nodes directly.
    /// </summary>
    protected override bool ShouldVisit(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return false;
    }

    /// <summary>
    /// Visits a plan node. This rule does not generate recommendations during node visitation.
    /// </summary>
    protected override IEnumerable<IndexRecommendation> VisitCore(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return Array.Empty<IndexRecommendation>();
    }

    /// <summary>
    /// Evaluates an execution plan for implicit conversions and returns index recommendations.
    /// </summary>
    public override IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        // Get conversion columns from statement text
        var conversionColumns = FindImplicitConversionColumns(plan);
        if (conversionColumns.Count == 0)
            return Array.Empty<IndexRecommendation>();

        // Find all tables involved in the plan with their nodes
        var tableNodes = plan.Nodes
            .Where(n => !string.IsNullOrEmpty(n.TableName))
            .GroupBy(n => n.TableName!)
            .ToList();

        if (tableNodes.Count == 0)
            return Array.Empty<IndexRecommendation>();

        var recommendations = new List<IndexRecommendation>();

        foreach (var tableGroup in tableNodes)
        {
            var table = tableGroup.Key;
            // Get predicate columns for this table
            var predicateColumns = tableGroup
                .SelectMany(n => n.PredicateColumns)
                .Distinct()
                .ToList();

            // Only include conversion columns that are also predicate columns
            var matchingColumns = conversionColumns
                .Where(c => predicateColumns.Contains(c, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (matchingColumns.Count == 0)
                continue;

            var confidence = tableGroup
                .Select(n => n.RelativeCost)
                .DefaultIfEmpty(0)
                .Max() switch
            {
                >= 0.30 => Confidence.High,
                >= 0.15 => Confidence.Medium,
                _ => Confidence.Low
            };

            recommendations.Add(new IndexRecommendation
            {
                Table = table,
                KeyColumns = matchingColumns,
                IncludeColumns = new List<string>(),
                EstimatedImpactPercent = EstimateImpact(tableGroup.FirstOrDefault()),
                SourceNodeCost = EstimateImpact(tableGroup.FirstOrDefault()) / 100.0,
                Confidence = confidence,
                Kind = RecommendationKind.SchemaFix,
                Reasons = new List<string>
                {
                    $"Query contains implicit conversion(s) on column(s): {string.Join(", ", matchingColumns)}"
                }
            });
        }

        return recommendations;
    }

    private static List<string> FindImplicitConversionColumns(ExecutionPlan plan)
    {
        var columns = new List<string>();

        if (string.IsNullOrEmpty(plan.StatementText))
            return columns;

        // SQL Server: CONVERT_IMPLICIT function in statement
        if (plan.StatementText.Contains("CONVERT_IMPLICIT", StringComparison.OrdinalIgnoreCase))
        {
            columns.AddRange(ExtractColumnsFromConversion(plan.StatementText));
        }

        // Postgres: type mismatch casts (:: operator with different types)
        if (plan.StatementText.Contains("::", StringComparison.Ordinal))
        {
            columns.AddRange(ExtractPostgresConversionColumns(plan.StatementText));
        }

        return columns.Distinct().ToList();
    }

    private static IEnumerable<string> ExtractColumnsFromConversion(string statementText)
    {
        var index = 0;
        while (index < statementText.Length)
        {
            var start = statementText.IndexOf("CONVERT_IMPLICIT", index, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                break;

            var parenStart = statementText.IndexOf('(', start);
            if (parenStart >= 0)
            {
                var parenEnd = FindMatchingParen(statementText, parenStart);
                if (parenEnd > parenStart)
                {
                    var expression = statementText.Substring(parenStart + 1, parenEnd - parenStart - 1);
                    // The second parameter after the comma is the column reference
                    var commaIndex = expression.IndexOf(',');
                    if (commaIndex >= 0)
                    {
                        var secondParam = expression.Substring(commaIndex + 1).Trim();
                        // Remove trailing parentheses content
                        var parenIdx = secondParam.IndexOf('(');
                        if (parenIdx >= 0)
                            secondParam = secondParam.Substring(0, parenIdx).Trim();
                        // Remove trailing closing parens
                        secondParam = secondParam.TrimEnd(')', ' ');
                        // Strip table/alias prefix (e.g., "orders.customer_id" -> "customer_id")
                        var dotIdx = secondParam.LastIndexOf('.');
                        if (dotIdx >= 0)
                            secondParam = secondParam.Substring(dotIdx + 1);
                        if (IsLikelyColumnName(secondParam))
                        {
                            yield return secondParam;
                        }
                    }
                    index = parenEnd + 1;
                    continue;
                }
            }
            index = start + "CONVERT_IMPLICIT".Length;
        }
    }

    private static IEnumerable<string> ExtractPostgresConversionColumns(string statementText)
    {
        var index = 0;
        while (index < statementText.Length)
        {
            var castIndex = statementText.IndexOf("::", index, StringComparison.Ordinal);
            if (castIndex < 0)
                break;

            // Extract the column name before ::
            var start = castIndex;
            while (start > 0 && !char.IsWhiteSpace(statementText[start - 1]) && statementText[start - 1] != ',')
            {
                start--;
            }

            var columnName = statementText.Substring(start, castIndex - start).Trim();
            if (!string.IsNullOrEmpty(columnName) && IsLikelyColumnName(columnName))
            {
                yield return columnName.Trim('[', ']');
            }

            index = castIndex + 2;
        }
    }

    private static bool IsLikelyColumnName(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return false;

        if (identifier.StartsWith('@') || identifier.Contains(' '))
            return false;

        return identifier.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    private static int FindMatchingParen(string text, int startPos)
    {
        var depth = 0;
        for (var i = startPos; i < text.Length; i++)
        {
            if (text[i] == '(') depth++;
            else if (text[i] == ')')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return text.Length - 1;
    }

    private static double EstimateImpact(PlanNode? node)
    {
        var cost = node?.RelativeCost ?? 0.5;
        return Math.Round(cost * 100.0, 1);
    }
}
