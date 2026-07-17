using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Contains unit tests for parsing SQL Server execution plans using the <see cref="SqlServerXmlPlanParser"/> class.
/// </summary>
public class SqlServerParserTests
{
    private const string Plan = """
    <?xml version="1.0" encoding="utf-16"?>
    <ShowPlanXML xmlns="http://schemas.microsoft.com/sqlserver/2004/07/showplan">
      <BatchSequence><Batch><Statements>
        <StmtSimple StatementText="SELECT * FROM dbo.Orders WHERE Status='open'" StatementSubTreeCost="4.0">
          <QueryPlan>
            <MissingIndexes>
              <MissingIndexGroup Impact="80">
                <MissingIndex Table="[Orders]" Schema="[dbo]">
                  <ColumnGroup Usage="EQUALITY"><Column Name="[Status]" /></ColumnGroup>
                  <ColumnGroup Usage="INCLUDE"><Column Name="[Total]" /></ColumnGroup>
                </MissingIndex>
              </MissingIndexGroup>
            </MissingIndexes>
            <RelOp PhysicalOp="Clustered Index Scan" EstimateRows="1000" EstimatedTotalSubtreeCost="4.0">
              <OutputList><ColumnReference Table="[Orders]" Column="Total" /></OutputList>
              <IndexScan>
                <Object Schema="[dbo]" Table="[Orders]" Index="[PK_Orders]" />
                <Predicate><ScalarOperator><ColumnReference Table="[Orders]" Column="Status" /></ScalarOperator></Predicate>
              </IndexScan>
            </RelOp>
          </QueryPlan>
        </StmtSimple>
      </Statements></Batch></BatchSequence>
    </ShowPlanXML>
    """;

    /// <summary>
    /// Tests that <see cref="SqlServerXmlPlanParser"/> correctly identifies SQL Server XML execution plans.
    /// </summary>
    [Fact]
    public void DetectsFormat()
    {
        Assert.True(new SqlServerXmlPlanParser().CanParse(Plan));
        Assert.False(new PostgresJsonPlanParser().CanParse(Plan));
    }

    /// <summary>
    /// Tests that <see cref="SqlServerXmlPlanParser"/> correctly parses SQL Server XML execution plan metadata.
    /// </summary>
    [Fact]
    public void ParsesDialectAndCost()
    {
        var plan = new SqlServerXmlPlanParser().Parse(Plan);
        Assert.Equal(PlanDialect.SqlServer, plan.Dialect);
        Assert.Equal(4.0, plan.EstimatedTotalCost, 3);
        Assert.Single(plan.Nodes);
    }

    /// <summary>
    /// Tests that <see cref="SqlServerXmlPlanParser"/> correctly extracts missing index recommendations from SQL Server XML execution plans.
    /// </summary>
    [Fact]
    public void ExtractsEngineMissingIndex()
    {
        var plan = new SqlServerXmlPlanParser().Parse(Plan);
        var hint = Assert.Single(plan.EngineMissingIndexes);
        Assert.Equal("dbo.Orders", hint.Table); // schema-qualified so it merges with the scan node
        Assert.Equal(new[] { "Status" }, hint.EqualityColumns);
        Assert.Equal(new[] { "Total" }, hint.IncludeColumns);
        Assert.Equal(80, hint.ImpactPercent);
    }

    /// <summary>
    /// Tests that <see cref="SqlServerXmlPlanParser"/> correctly extracts predicate and output columns from SQL Server XML execution plan nodes.
    /// </summary>
    [Fact]
    public void ExtractsScanPredicateColumns()
    {
        var plan = new SqlServerXmlPlanParser().Parse(Plan);
        var node = plan.Nodes[0];
        Assert.Equal("dbo.Orders", node.TableName);
        Assert.Contains("Status", node.PredicateColumns);
        Assert.Contains("Total", node.OutputColumns);
    }
}
