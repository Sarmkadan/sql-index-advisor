using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Core.Model;

public enum SqlDialect { SqlServer, Postgres }

public static class RecommendationScriptExtensions
{
    public static string ToCreateIndexSql(this IndexRecommendation recommendation, SqlDialect dialect)
    {
        var tableName = recommendation.Table;
        var keyColumns = string.Join(", ", recommendation.KeyColumns);
        var includeColumns = string.Join(", ", recommendation.IncludeColumns);

        var indexName = $"IX_{tableName}_{string.Join("_", recommendation.KeyColumns)}";

        switch (dialect)
        {
            case SqlDialect.SqlServer:
                return $"CREATE NONCLUSTERED INDEX {indexName} ON {tableName} ({keyColumns}) INCLUDE ({includeColumns});";
            case SqlDialect.Postgres:
                return $"CREATE INDEX {indexName} ON {tableName} ({keyColumns}) INCLUDE ({includeColumns});";
            default:
                throw new ArgumentOutOfRangeException(nameof(dialect), dialect, null);
        }
    }
}
