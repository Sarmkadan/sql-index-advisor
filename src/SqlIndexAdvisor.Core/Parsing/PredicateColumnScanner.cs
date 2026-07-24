using System.Text.RegularExpressions;

namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Extracts column names from a Postgres predicate expression string.
/// This is intentionally simple - it grabs the identifier on the left of a
/// comparison operator. It won't understand function calls wrapping a column
/// (e.g. lower(name) = ...), and that's fine: such predicates aren't
/// sargable anyway so skipping them is the correct behavior.
/// </summary>
public static partial class PredicateColumnScanner
{
    // matches:  optional "alias." then identifier, then a comparison operator
    [GeneratedRegex(@"(?:[A-Za-z_][A-Za-z0-9_]*\.)?([A-Za-z_][A-Za-z0-9_]*)\s*(?:=|<>|!=|<=|>=|<|>|~|LIKE|ILIKE|IS|IS NOT|IN|NOT IN|BETWEEN|NOT BETWEEN)\s*(?:[A-Za-z_][A-Za-z0-9_]*)?",
        RegexOptions.IgnoreCase)]
    private static partial Regex PredicateRegex();

    private static readonly HashSet<string> Noise = new(StringComparer.OrdinalIgnoreCase)
    {
        "AND", "OR", "NOT", "NULL", "TRUE", "FALSE", "ANY", "ALL"
    };

    private static string GetQuotedIdentifier(string identifier, bool isPostgres)
    {
        if (isPostgres)
        {
            if (identifier.StartsWith("\"") && identifier.EndsWith("\""))
                return identifier;
            return $"\"{identifier}\"";
        }
        return identifier;
    }

    private static bool IsPostgres(string expression)
    {
        return expression.Contains("\"Node Type\"", StringComparison.OrdinalIgnoreCase)
            || expression.Contains("\"Plan\"", StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<string> Scan(string expression)
    {
        var isPostgres = IsPostgres(expression);
        return Scan(expression, isPostgres);
    }

    public static IEnumerable<string> Scan(string expression, bool isPostgres)
    {
        foreach (Match m in PredicateRegex().Matches(expression))
        {
            var col = m.Groups[1].Value;
            var quotedIdentifier = GetQuotedIdentifier(col, isPostgres);
            if (!Noise.Contains(quotedIdentifier)) continue;
            yield return quotedIdentifier;
        }
    }
}
