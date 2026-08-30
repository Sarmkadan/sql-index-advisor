using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Tests for dialect-specific index DDL rendering.
/// </summary>
public class DdlRendererTests
{
    [Fact]
    public void RenderCreateIndex_SqlServer_RendersQuotedIdentifiersIncludesAndOnlineOption()
    {
        // Arrange
        var recommendation = CreateRecommendation(
            "sales.Order Details",
            new() { "Order", "Customer Id" },
            new() { "Unit Price" });

        // Act
        var ddl = DdlRenderer.RenderCreateIndex(recommendation, PlanDialect.SqlServer);

        // Assert
        Assert.StartsWith("CREATE NONCLUSTERED INDEX ", ddl);
        Assert.Contains(" ON sales.[Order Details] ([Order], [Customer Id])", ddl);
        Assert.Contains(" INCLUDE ([Unit Price])", ddl);
        Assert.EndsWith(" WITH (ONLINE = ON);", ddl);
    }

    [Fact]
    public void RenderCreateIndex_SqlServer_WithNoIncludeColumns_OmitsIncludeClause()
    {
        // Arrange
        var recommendation = CreateRecommendation("Orders", new() { "CustomerId" });

        // Act
        var ddl = DdlRenderer.RenderCreateIndex(recommendation, PlanDialect.SqlServer);

        // Assert
        Assert.DoesNotContain(" INCLUDE ", ddl);
        Assert.EndsWith(" WITH (ONLINE = ON);", ddl);
    }

    [Fact]
    public void RenderCreateIndex_Postgres_RendersExpectedShapeAndLowercasePrefix()
    {
        // Arrange
        var recommendation = CreateRecommendation(
            "sales.Order Details",
            new() { "Order", "Customer Id" },
            new() { "Unit Price" });

        // Act
        var ddl = DdlRenderer.RenderCreateIndex(recommendation, PlanDialect.Postgres);
        var indexName = GetDelimitedIndexName(ddl, "CREATE INDEX CONCURRENTLY ");

        // Assert
        Assert.StartsWith("ix_", indexName);
        Assert.Contains(" ON sales.\"Order Details\" (\"Order\", \"Customer Id\")", ddl);
        Assert.Contains(" INCLUDE (\"Unit Price\")", ddl);
        Assert.EndsWith(";", ddl);
        Assert.DoesNotContain("WITH (ONLINE = ON)", ddl);
    }

    [Fact]
    public void RenderCreateIndex_LongGeneratedName_UsesDialectSpecificLengthLimit()
    {
        // Arrange
        var recommendation = CreateRecommendation(
            new string('T', 80),
            new() { new string('C', 80) });

        // Act
        var sqlServerDdl = DdlRenderer.RenderCreateIndex(recommendation, PlanDialect.SqlServer);
        var postgresDdl = DdlRenderer.RenderCreateIndex(recommendation, PlanDialect.Postgres);
        var sqlServerIndexName = GetDelimitedIndexName(sqlServerDdl, "CREATE NONCLUSTERED INDEX ");
        var postgresIndexName = GetDelimitedIndexName(postgresDdl, "CREATE INDEX CONCURRENTLY ");

        // Assert
        Assert.StartsWith("IX_", sqlServerIndexName);
        Assert.Equal(128, sqlServerIndexName.Length);
        Assert.StartsWith("ix_", postgresIndexName);
        Assert.Equal(63, postgresIndexName.Length);
    }

    [Fact]
    public void RenderCreateIndex_NullRecommendation_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => DdlRenderer.RenderCreateIndex(null!, PlanDialect.SqlServer));
        Assert.Equal("recommendation", exception.ParamName);
    }

    [Fact]
    public void RenderCreateIndex_UnsupportedDialect_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var recommendation = CreateRecommendation("Orders", new() { "CustomerId" });

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => DdlRenderer.RenderCreateIndex(recommendation, (PlanDialect)int.MaxValue));
        Assert.Equal("dialect", exception.ParamName);
    }

    private static IndexRecommendation CreateRecommendation(
        string table,
        List<string> keyColumns,
        List<string>? includeColumns = null)
    {
        return new IndexRecommendation
        {
            Table = table,
            KeyColumns = keyColumns,
            IncludeColumns = includeColumns ?? new()
        };
    }

    private static string GetDelimitedIndexName(string ddl, string prefix)
    {
        var nameStart = prefix.Length;
        var nameEnd = ddl.IndexOf(" ON ", nameStart, StringComparison.Ordinal);
        return ddl[nameStart..nameEnd].Trim('"', '[', ']');
    }
}
