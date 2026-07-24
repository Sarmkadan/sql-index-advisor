using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Provides dialect-specific DDL generation for index recommendations.
/// Generates ready-to-run, copy-pasteable CREATE INDEX statements with safe options.
/// </summary>
public static class DdlRenderer
{
    private const int SqlServerMaxIndexNameLength = 128;
    private const int PostgresMaxIndexNameLength = 63;
    private const string SqlServerIndexNamePrefix = "IX_";
    private const string PostgresIndexNamePrefix = "ix_";

    /// <summary>
    /// Generates a dialect-specific CREATE INDEX statement from an index recommendation.
    /// </summary>
    /// <param name="recommendation">The index recommendation to render as DDL.</param>
    /// <param name="dialect">The target SQL dialect (SQL Server or Postgres).</param>
    /// <returns>A ready-to-run CREATE INDEX statement string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when recommendation is null.</exception>
    public static string RenderCreateIndex(IndexRecommendation recommendation, PlanDialect dialect)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        return dialect switch
        {
            PlanDialect.SqlServer => RenderSqlServerCreateIndex(recommendation),
            PlanDialect.Postgres => RenderPostgresCreateIndex(recommendation),
            _ => throw new ArgumentOutOfRangeException(nameof(dialect), dialect, "Unsupported SQL dialect")
        };
    }

    /// <summary>
    /// Generates a SQL Server-specific CREATE INDEX statement.
    /// </summary>
    private static string RenderSqlServerCreateIndex(IndexRecommendation recommendation)
    {
        var sb = new StringBuilder();

        // Use existing index name if provided, otherwise generate one
        var indexName = GetSqlServerIndexName(recommendation);

        sb.Append("CREATE NONCLUSTERED INDEX ")
           .Append(QuoteSqlServerIdentifier(indexName))
           .Append(" ON ")
           .Append(QuoteSqlServerIdentifier(recommendation.Table))
           .Append(" (");

        // Key columns
        AppendQuotedColumnList(sb, recommendation.KeyColumns, QuoteSqlServerIdentifier);
        sb.Append(")");

        // INCLUDE clause
        if (recommendation.IncludeColumns.Count > 0)
        {
            sb.Append(" INCLUDE (");
            AppendQuotedColumnList(sb, recommendation.IncludeColumns, QuoteSqlServerIdentifier);
            sb.Append(")");
        }

        // WITH options for safe online creation
        sb.Append(" WITH (ONLINE = ON)");

        sb.Append(';');
        return sb.ToString();
    }

    /// <summary>
    /// Generates a PostgreSQL-specific CREATE INDEX statement.
    /// </summary>
    private static string RenderPostgresCreateIndex(IndexRecommendation recommendation)
    {
        var sb = new StringBuilder();

        // Use CONCURRENTLY for safe online creation
        sb.Append("CREATE INDEX CONCURRENTLY ");

        var indexName = GetPostgresIndexName(recommendation);
        sb.Append(QuotePostgresIdentifier(indexName))
           .Append(" ON ")
           .Append(QuotePostgresIdentifier(recommendation.Table))
           .Append(" (");

        // Key columns
        AppendQuotedColumnList(sb, recommendation.KeyColumns, QuotePostgresIdentifier);
        sb.Append(")");

        // INCLUDE clause (PostgreSQL 11+)
        if (recommendation.IncludeColumns.Count > 0)
        {
            sb.Append(" INCLUDE (");
            AppendQuotedColumnList(sb, recommendation.IncludeColumns, QuotePostgresIdentifier);
            sb.Append(")");
        }

        sb.Append(';');
        return sb.ToString();
    }

    /// <summary>
    /// Generates a SQL Server-compatible index name with length limit and hash suffix.
    /// </summary>
    private static string GetSqlServerIndexName(IndexRecommendation recommendation)
    {
        var baseName = recommendation.SuggestedName();
        return TruncateIndexName(baseName, SqlServerMaxIndexNameLength, SqlServerIndexNamePrefix);
    }

    /// <summary>
    /// Generates a PostgreSQL-compatible index name with length limit and hash suffix.
    /// </summary>
    private static string GetPostgresIndexName(IndexRecommendation recommendation)
    {
        // PostgreSQL prefers lowercase index names
        var baseName = recommendation.SuggestedName()?.ToLowerInvariant();
        return TruncateIndexName(baseName, PostgresMaxIndexNameLength, PostgresIndexNamePrefix);
    }

    /// <summary>
    /// Truncates an index name to fit dialect-specific limits, adding a hash suffix if truncated.
    /// </summary>
    /// <param name="baseName">The base index name.</param>
    /// <param name="maxLength">Maximum allowed length for the dialect.</param>
    /// <param name="prefix">The required prefix for the dialect.</param>
    /// <returns>A truncated index name with hash suffix if needed.</returns>
    private static string TruncateIndexName(string? baseName, int maxLength, string prefix)
    {
        if (string.IsNullOrEmpty(baseName))
        {
            return prefix + "_generated";
        }

        // Ensure the name starts with the required prefix
        var name = baseName.StartsWith(prefix, StringComparison.Ordinal) ? baseName : prefix + baseName;

        if (name.Length <= maxLength)
        {
            return name;
        }

        // Truncate to max length minus hash suffix length (8 chars for "_XXXXXX")
        var availableLength = maxLength - 8;
        if (availableLength <= 0)
        {
            availableLength = maxLength - 1;
        }

        var truncated = name[..availableLength];
        var hashSuffix = GenerateShortHash(name);
        return $"{truncated}_{hashSuffix}";
    }

    /// <summary>
    /// Generates a short hash suffix for index names that were truncated.
    /// </summary>
    private static string GenerateShortHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        var hashString = Convert.ToBase64String(hashBytes)
            .Replace("+", "", StringComparison.Ordinal)
            .Replace("/", "", StringComparison.Ordinal)
            .Replace("=", "", StringComparison.Ordinal);

        // Take first 6 characters for a short, unique suffix
        return hashString[..Math.Min(6, hashString.Length)];
    }

    /// <summary>
    /// Appends a list of column names to a StringBuilder with proper quoting.
    /// </summary>
    private static void AppendQuotedColumnList(StringBuilder sb, IReadOnlyList<string> columns, Func<string, string> quoteFunc)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }
            sb.Append(quoteFunc(columns[i]));
        }
    }

    /// <summary>
    /// Quotes a SQL Server identifier using square brackets.
    /// </summary>
    private static string QuoteSqlServerIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return identifier;
        }

        // Check if already quoted
        if (identifier.StartsWith("[", StringComparison.Ordinal) && identifier.EndsWith("]", StringComparison.Ordinal))
        {
            return identifier;
        }

        // Handle multi-part identifiers (schema.table)
        var parts = identifier.Split('.', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            // Skip empty parts and handle them
            if (string.IsNullOrEmpty(parts[i]))
            {
                continue;
            }

            // Only quote if it's not a simple identifier
            if (NeedsQuoting(parts[i]))
            {
                parts[i] = $"[{EscapeBrackets(parts[i])}]";
            }
        }

        return string.Join(".", parts);
    }

    /// <summary>
    /// Quotes a PostgreSQL identifier using double quotes.
    /// </summary>
    private static string QuotePostgresIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return identifier;
        }

        // Check if already quoted
        if (identifier.StartsWith("\"", StringComparison.Ordinal) && identifier.EndsWith("\"", StringComparison.Ordinal))
        {
            return identifier;
        }

        // Handle multi-part identifiers (schema.table)
        var parts = identifier.Split('.', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            if (string.IsNullOrEmpty(parts[i]))
            {
                continue;
            }

            if (NeedsQuoting(parts[i]))
            {
                parts[i] = $"\"{EscapeDoubleQuotes(parts[i])}\"";
            }
        }

        return string.Join(".", parts);
    }

    /// <summary>
    /// Determines if an identifier needs quoting.
    /// </summary>
    private static bool NeedsQuoting(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return false;
        }

        // Check if it's a simple identifier (letters, digits, underscores, starts with letter/underscore)
        if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
        {
            return true;
        }

        // Check for invalid characters
        foreach (var c in identifier)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                return true;
            }
        }

        // Check for SQL Server keywords (simplified check)
        var sqlServerKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ADD", "ALL", "ALTER", "AND", "ANY", "AS", "ASC", "BACKUP", "BEGIN", "BETWEEN",
            "BREAK", "BROWSE", "BULK", "BY", "CASCADE", "CASE", "CHECK", "CHECKPOINT", "CLOSE",
            "CLUSTERED", "COALESCE", "COLLATE", "COLUMN", "COMMIT", "COMPUTE", "CONSTRAINT",
            "CONTAINS", "CONTAINSTABLE", "CONTINUE", "CONVERT", "CREATE", "CROSS", "CURRENT",
            "CURRENT_DATE", "CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_USER", "CURSOR",
            "DATABASE", "DBCC", "DEALLOCATE", "DECLARE", "DEFAULT", "DELETE", "DENY", "DESC",
            "DISK", "DISTINCT", "DISTRIBUTED", "DOUBLE", "DROP", "DUMP", "ELSE", "END",
            "ERRLVL", "ESCAPE", "EXCEPT", "EXEC", "EXECUTE", "EXISTS", "EXIT", "EXTERNAL", "FETCH",
            "FILE", "FILLFACTOR", "FOR", "FOREIGN", "FREETEXT", "FREETEXTTABLE", "FROM", "FULL",
            "FUNCTION", "GOTO", "GRANT", "GROUP", "HAVING", "HOLDLOCK", "IDENTITY", "IDENTITY_INSERT",
            "IDENTITYCOL", "IF", "IN", "INDEX", "INNER", "INSERT", "INTERSECT", "INTO", "IS", "JOIN",
            "KEY", "KILL", "LEFT", "LIKE", "LINENO", "LOAD", "MERGE", "NATIONAL", "NOCHECK", "NONCLUSTERED",
            "NOT", "NULL", "NULLIF", "OF", "OFFSETS", "ON", "OPEN", "OPEND", "OPTION", "OR", "ORDER",
            "OUTER", "OVER", "PERCENT", "PIVOT", "PLAN", "PRECISION", "PRIMARY", "PRINT", "PROC", "PROCEDURE",
            "PUBLIC", "RAISERROR", "READ", "READTEXT", "RECONFIGURE", "REFERENCES", "REPLICATION", "RESTORE",
            "RESTRICT", "RETURN", "REVOKE", "RIGHT", "ROLLBACK", "ROWCOUNT", "ROWGUIDCOL", "RULE", "SAVE",
            "SCHEMA", "SECURITYAUDIT", "SELECT", "SEMANTICKEYPHRASETABLE", "SEMANTICTABLEKEYPHRASETABLE",
            "SESSION_USER", "SET", "SETUSER", "SHUTDOWN", "SOME", "STATISTICS", "SYSTEM_USER", "TABLE",
            "TABLESAMPLE", "TEXTSIZE", "THEN", "TO", "TOP", "TRAN", "TRANSACTION", "TRIGGER", "TRUNCATE",
            "TRY_CONVERT", "TSEQUAL", "UNION", "UNIQUE", "UNPIVOT", "UPDATE", "UPDATETEXT", "USE", "USER",
            "VALUES", "VARYING", "VIEW", "WAITFOR", "WHEN", "WHERE", "WHILE", "WITH", "WRITETEXT"
        };

        return sqlServerKeywords.Contains(identifier);
    }

    /// <summary>
    /// Escapes square brackets in an identifier for SQL Server.
    /// </summary>
    private static string EscapeBrackets(string identifier)
    {
        return identifier.Replace("]", "]]", StringComparison.Ordinal);
    }

    /// <summary>
    /// Escapes double quotes in an identifier for PostgreSQL.
    /// </summary>
    private static string EscapeDoubleQuotes(string identifier)
    {
        return identifier.Replace("\"", "\"\"", StringComparison.Ordinal);
    }
}