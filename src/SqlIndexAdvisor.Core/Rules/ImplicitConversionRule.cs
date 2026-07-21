using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Detects implicit conversions in predicate expressions that can prevent index usage.
/// Flags CONVERT_IMPLICIT operations in SQL Server and type-mismatch casts in Postgres.
/// Recommends aligning column/parameter types to enable index seeks.
/// </summary>
public sealed class ImplicitConversionRule : IIndexRule
{
    public string Name => "implicit-conversion";

    public IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan)
    {
        // Check the statement text for implicit conversions
        var conversionColumns = FindImplicitConversionColumns(plan);
        if (conversionColumns.Count == 0)
            yield break;

        // Find all tables involved in the plan
        var tablesInPlan = plan.Nodes
            .Where(n => !string.IsNullOrEmpty(n.TableName))
            .Select(n => n.TableName!)
            .Distinct()
            .ToList();

        // If we have tables in the plan, associate the conversions with them
        foreach (var table in tablesInPlan)
        {
            var confidence = plan.Nodes
                .Where(n => n.TableName == table)
                .Select(n => n.RelativeCost)
                .DefaultIfEmpty(0)
                .Max() switch
            {
                >= 0.30 => Confidence.High,
                >= 0.15 => Confidence.Medium,
                _ => Confidence.Low
            };

            yield return new IndexRecommendation
            {
                Table = table,
                KeyColumns = conversionColumns,
                IncludeColumns = [],
                EstimatedImpactPercent = EstimateImpact(plan.Nodes.FirstOrDefault()),
                SourceNodeCost = EstimateImpact(plan.Nodes.FirstOrDefault()) / 100.0,
                Confidence = confidence,
                Reasons = new List<string>
                {
                    $"Query contains implicit conversion(s) on column(s): {string.Join(", ", conversionColumns)}"
                }
            };
        }
    }

    private static List<string> FindImplicitConversionColumns(ExecutionPlan plan)
    {
        var columns = new List<string>();

        // SQL Server: CONVERT_IMPLICIT function in statement
        if (!string.IsNullOrEmpty(plan.StatementText))
        {
            // Look for CONVERT_IMPLICIT pattern
            if (plan.StatementText.Contains("CONVERT_IMPLICIT", StringComparison.OrdinalIgnoreCase))
            {
                // Extract column names involved in the conversion
                columns.AddRange(ExtractColumnsFromConversion(plan.StatementText));
            }
        }

        // Postgres: type mismatch casts (:: operator with different types)
        // Look for patterns like "column::different_type" or "CAST(column AS different_type)"
        if (!string.IsNullOrEmpty(plan.StatementText) && plan.StatementText.Contains("::", StringComparison.Ordinal))
        {
            columns.AddRange(ExtractPostgresConversionColumns(plan.StatementText));
        }

        return columns.Distinct().ToList();
    }

    private static IEnumerable<string> ExtractColumnsFromConversion(string statementText)
    {
        // Simple extraction: look for column references in CONVERT_IMPLICIT expressions
        // Pattern: CONVERT_IMPLICIT(type, expression)
        var start = statementText.IndexOf("CONVERT_IMPLICIT", StringComparison.OrdinalIgnoreCase);
        if (start >= 0)
        {
            // Extract the expression part (second parameter)
            var parenStart = statementText.IndexOf('(', start);
            if (parenStart >= 0)
            {
                var parenEnd = FindMatchingParen(statementText, parenStart);
                if (parenEnd > parenStart)
                {
                    var expression = statementText.Substring(parenStart + 1, parenEnd - parenStart - 1);
                    // Extract column references from the expression
                    // Simple approach: look for identifiers that look like column names
                    var parts = expression.Split(new[] { ',', ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (IsLikelyColumnName(part) && !part.StartsWith('@') && !part.StartsWith('[') && !part.EndsWith(']'))
                        {
                            yield return part.Trim('[', ']');
                        }
                    }
                }
            }
        }
    }

    private static IEnumerable<string> ExtractPostgresConversionColumns(string statementText)
    {
        // Look for :: casts with type mismatches
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

        // Also check for CAST expressions
        index = 0;
        while (index < statementText.Length)
        {
            var castIndex = statementText.IndexOf("CAST(", index, StringComparison.OrdinalIgnoreCase);
            if (castIndex < 0)
                break;

            var parenStart = castIndex + 4;
            var parenEnd = FindMatchingParen(statementText, parenStart - 1);
            if (parenEnd > parenStart)
            {
                var castContent = statementText.Substring(parenStart, parenEnd - parenStart);
                // Extract column reference from CAST(expression AS type)
                var asIndex = castContent.IndexOf(" AS ", StringComparison.OrdinalIgnoreCase);
                if (asIndex > 0)
                {
                    var columnRef = castContent.Substring(0, asIndex).Trim();
                    if (IsLikelyColumnName(columnRef))
                    {
                        yield return columnRef.Trim('[', ']');
                    }
                }
            }

            index = parenEnd + 1;
        }
    }

    private static bool IsLikelyColumnName(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return false;

        // Column names typically contain letters, numbers, underscores
        // and don't start with @ (parameter) or contain spaces
        if (identifier.StartsWith('@') || identifier.Contains(' '))
            return false;

        // Check if it looks like a column name (not a keyword, not a number)
        return identifier.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    private static int FindMatchingParen(string text, int startPos)
    {
        var depth = 0;
        for (var i = startPos; i < text.Length; i++)
        {
            if (text[i] == '(')
            {
                depth++;
            }
            else if (text[i] == ')')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }
        return text.Length - 1;
    }

    /// <summary>
    /// Estimates impact: implicit conversions can prevent index usage entirely,
    /// forcing scans. Impact scales with the cost of the node.
    /// </summary>
    private static double EstimateImpact(PlanNode node)
    {
        // Implicit conversions are often performance killers - high impact
        return Math.Round(node.RelativeCost * 100.0, 1);
    }
}