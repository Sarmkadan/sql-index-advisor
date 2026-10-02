using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Extracts column names from a Postgres predicate expression string.
/// </summary>
public static partial class PredicateColumnScanner
{
    // Matches identifier (optionally qualified) followed by comparison operator
    [GeneratedRegex(@"(?:(?:""[^""]+""|\b[A-Za-z_][A-Za-z0-9_]*\b)\.)?(?:""([^""]+)""|(\b[A-Za-z_][A-Za-z0-9_]*\b))\s*(?:=|<>|!=|<=|>=|<|>|~|\bLIKE\b|\bILIKE\b|\bIS\s+NOT\b|\bIS\b|\bNOT\s+IN\b|\bIN\b|\bNOT\s+BETWEEN\b|\bBETWEEN\b)(?=[\s(']|$)",
    RegexOptions.IgnoreCase)]
    private static partial Regex PredicateRegex();

    private static readonly HashSet<string> Noise = new(StringComparer.OrdinalIgnoreCase)
    {
        "AND", "OR", "NOT", "NULL", "TRUE", "FALSE", "ANY", "ALL"
    };

    private static readonly HashSet<string> FunctionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "UPPER", "LOWER", "TRIM", "LTRIM", "RTRIM", "SUBSTRING", "CONCAT", "COALESCE",
        "ISNULL", "NULLIF", "CAST", "CONVERT", "CONVERT_IMPLICIT", "REPLACE", "CHARINDEX",
        "LEN", "LENGTH", "LEFT", "RIGHT", "REVERSE", "STUFF", "FORMAT",
        "ABS", "CEILING", "FLOOR", "ROUND", "POWER", "SQRT", "LOG", "EXP",
        "GETDATE", "GETUTCDATE", "DATEADD", "DATEDIFF", "DATEPART", "YEAR", "MONTH", "DAY",
        "NOW", "CURRENT_TIMESTAMP", "DATE_TRUNC", "EXTRACT",
        "COUNT", "SUM", "AVG", "MIN", "MAX", "STRING_AGG", "ARRAY_AGG"
    };

    /// <summary>
    /// Extracts column names from the given predicate expression.
    /// </summary>
    public static IEnumerable<string> Scan(string expression)
    {
        if (expression is null)
            return Array.Empty<string>();
        var isPostgres = IsPostgres(expression);
        return ScanInternal(expression, isPostgres);
    }

    /// <summary>
    /// Extracts column names from the given predicate expression, specifying whether the expression is in Postgres format.
    /// </summary>
    public static IEnumerable<string> Scan(string expression, bool isPostgres)
    {
        if (expression is null)
            return Array.Empty<string>();
        return ScanInternal(expression, isPostgres);
    }

    private static bool IsPostgres(string expression)
    {
        if (expression == null)
            return false;
        return expression.Contains("\"Node Type\"", StringComparison.OrdinalIgnoreCase)
               || expression.Contains("\"Plan\"", StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> ScanInternal(string expression, bool isPostgres)
    {
        var results = new List<string>();
        if (string.IsNullOrWhiteSpace(expression))
            return results;

        // Build set of positions that are inside function calls
        var insideFunction = BuildFunctionCallPositions(expression);

        foreach (Match m in PredicateRegex().Matches(expression))
        {
            string col;
            int colStart;
            if (m.Groups[1].Success)
            {
                col = m.Groups[1].Value;
                colStart = m.Groups[1].Index;
            }
            else if (m.Groups[2].Success)
            {
                col = m.Groups[2].Value;
                colStart = m.Groups[2].Index;
            }
            else
            {
                continue;
            }

            // Skip if inside a function call
            if (insideFunction.Contains(colStart))
                continue;

            // Skip if preceded by an arithmetic operator in an expression context
            if (IsPrecededByArithmetic(expression, m.Index))
                continue;

            // Filter noise words
            if (Noise.Contains(col))
                continue;

            // For postgres, wrap in quotes
            var result = isPostgres ? $"\"{col}\"" : col;
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Identifies positions inside function calls (e.g., UPPER(...), TRIM(...)).
    /// Only treats known SQL function names as functions, not AND/OR/NOT etc.
    /// </summary>
    private static HashSet<int> BuildFunctionCallPositions(string expression)
    {
        var positions = new HashSet<int>();
        var funcRegex = new Regex(@"\b([A-Za-z_][A-Za-z0-9_]*)\s*\(", RegexOptions.IgnoreCase);

        foreach (Match fm in funcRegex.Matches(expression))
        {
            var funcName = fm.Groups[1].Value;
            // Only treat known function names as function calls
            if (!FunctionNames.Contains(funcName))
                continue;

            // Find the opening paren position
            var parenPos = fm.Value.LastIndexOf('(');
            var openParen = fm.Index + parenPos;

            // Find matching close paren
            var depth = 0;
            for (var i = openParen; i < expression.Length; i++)
            {
                if (expression[i] == '(') depth++;
                else if (expression[i] == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        for (var j = fm.Index; j <= i; j++)
                            positions.Add(j);
                        break;
                    }
                }
            }
        }

        return positions;
    }

    /// <summary>
    /// Checks if the match position is preceded by an arithmetic operator in expression context.
    /// </summary>
    private static bool IsPrecededByArithmetic(string expression, int matchStart)
    {
        for (var i = matchStart - 1; i >= 0; i--)
        {
            var c = expression[i];
            if (char.IsWhiteSpace(c)) continue;
            if (c == '*' || c == '/' || c == '+')
            {
                // Verify it's in arithmetic context (preceded by identifier/number/close-paren)
                for (var j = i - 1; j >= 0; j--)
                {
                    var prev = expression[j];
                    if (char.IsWhiteSpace(prev)) continue;
                    if (char.IsLetterOrDigit(prev) || prev == '_' || prev == ')')
                        return true;
                    break;
                }
            }
            // '-' could be unary minus, skip as not reliable
            break;
        }
        return false;
    }
}
