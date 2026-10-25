using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Contains tests that verify <see cref="SqlServerXmlPlanParser"/> tolerates missing optional attributes and elements.
/// </summary>
public class SqlServerParserMissingAttributesTests
{
    private static string Wrap(string statement) =>
        $"""
        <ShowPlanXML xmlns="http://schemas.microsoft.com/sqlserver/2004/07/showplan">
          <BatchSequence><Batch><Statements>
            {statement}
          </Statements></Batch></BatchSequence>
        </ShowPlanXML>
        """;

    /// <summary>
    /// Tests that a statement without text or cost attributes parses with empty text and zero cost.
    /// </summary>
    [Fact]
    public void StatementWithoutTextOrCostUsesDefaults()
    {
        var xml = Wrap("""
            <StmtSimple>
              <QueryPlan>
                <RelOp PhysicalOp="Table Scan" EstimateRows="10" EstimatedTotalSubtreeCost="1.0" />
              </QueryPlan>
            </StmtSimple>
            """);

        var plan = new SqlServerXmlPlanParser().Parse(xml).Single();

        Assert.Equal(string.Empty, plan.StatementText);
        Assert.Equal(0, plan.EstimatedTotalCost);
        Assert.Equal(0, plan.Nodes[0].RelativeCost);
    }

    /// <summary>
    /// Tests that an operator without any attributes falls back to defaults instead of throwing.
    /// </summary>
    [Fact]
    public void RelOpWithoutAttributesUsesDefaults()
    {
        var xml = Wrap("""
            <StmtSimple StatementText="SELECT 1" StatementSubTreeCost="2.0">
              <QueryPlan>
                <RelOp />
              </QueryPlan>
            </StmtSimple>
            """);

        var node = new SqlServerXmlPlanParser().Parse(xml).Single().Nodes.Single();

        Assert.Equal("Unknown", node.Operator);
        Assert.Equal(0, node.EstimatedRows);
        Assert.Equal(0, node.EstimatedRowsRead);
        Assert.Equal(0, node.TableCardinality);
        Assert.Equal(0, node.ActualRows);
        Assert.Equal(0, node.ActualRowsRead);
        Assert.Equal(0, node.RelativeCost);
        Assert.Null(node.TableName);
        Assert.Null(node.IndexName);
        Assert.Empty(node.PredicateColumns);
        Assert.Empty(node.OutputColumns);
    }

    /// <summary>
    /// Tests that an <c>Object</c> element without table or index attributes yields no object names.
    /// </summary>
    [Fact]
    public void ObjectWithoutNamesYieldsNullNames()
    {
        var xml = Wrap("""
            <StmtSimple StatementText="SELECT 1" StatementSubTreeCost="2.0">
              <QueryPlan>
                <RelOp PhysicalOp="Index Scan" EstimateRows="5" EstimatedTotalSubtreeCost="1.0">
                  <IndexScan>
                    <Object />
                  </IndexScan>
                </RelOp>
              </QueryPlan>
            </StmtSimple>
            """);

        var node = new SqlServerXmlPlanParser().Parse(xml).Single().Nodes.Single();

        Assert.Null(node.TableName);
        Assert.Null(node.IndexName);
    }

    /// <summary>
    /// Tests that an <c>Object</c> element with only a table name keeps that name without a schema prefix.
    /// </summary>
    [Fact]
    public void ObjectWithoutSchemaUsesBareTableName()
    {
        var xml = Wrap("""
            <StmtSimple StatementText="SELECT 1" StatementSubTreeCost="2.0">
              <QueryPlan>
                <RelOp PhysicalOp="Index Scan" EstimateRows="5" EstimatedTotalSubtreeCost="1.0">
                  <IndexScan>
                    <Object Table="[Orders]" />
                  </IndexScan>
                </RelOp>
              </QueryPlan>
            </StmtSimple>
            """);

        var node = new SqlServerXmlPlanParser().Parse(xml).Single().Nodes.Single();

        Assert.Equal("Orders", node.TableName);
        Assert.Null(node.IndexName);
    }

    /// <summary>
    /// Tests that a missing-index hint without a table name or columns is kept with empty values instead of throwing.
    /// </summary>
    [Fact]
    public void MissingIndexWithoutTableOrColumnsDoesNotThrow()
    {
        var xml = Wrap("""
            <StmtSimple StatementText="SELECT 1" StatementSubTreeCost="2.0">
              <QueryPlan>
                <MissingIndexes>
                  <MissingIndexGroup>
                    <MissingIndex />
                  </MissingIndexGroup>
                </MissingIndexes>
                <RelOp PhysicalOp="Table Scan" EstimateRows="5" EstimatedTotalSubtreeCost="1.0" />
              </QueryPlan>
            </StmtSimple>
            """);

        var hint = Assert.Single(new SqlServerXmlPlanParser().Parse(xml).Single().EngineMissingIndexes);

        Assert.Equal(string.Empty, hint.Table);
        Assert.Equal(0, hint.ImpactPercent);
        Assert.Empty(hint.EqualityColumns);
        Assert.Empty(hint.InequalityColumns);
        Assert.Empty(hint.IncludeColumns);
    }

    /// <summary>
    /// Tests that column references and column groups without names are skipped rather than added as empty strings.
    /// </summary>
    [Fact]
    public void NamelessColumnsAreSkipped()
    {
        var xml = Wrap("""
            <StmtSimple StatementText="SELECT 1" StatementSubTreeCost="2.0">
              <QueryPlan>
                <MissingIndexes>
                  <MissingIndexGroup Impact="50">
                    <MissingIndex Table="[Orders]">
                      <ColumnGroup Usage="EQUALITY"><Column /></ColumnGroup>
                      <ColumnGroup><Column Name="[Total]" /></ColumnGroup>
                    </MissingIndex>
                  </MissingIndexGroup>
                </MissingIndexes>
                <RelOp PhysicalOp="Index Scan" EstimateRows="5" EstimatedTotalSubtreeCost="1.0">
                  <OutputList><ColumnReference Table="[Orders]" /></OutputList>
                  <IndexScan>
                    <Predicate><ScalarOperator><ColumnReference /></ScalarOperator></Predicate>
                  </IndexScan>
                </RelOp>
              </QueryPlan>
            </StmtSimple>
            """);

        var plan = new SqlServerXmlPlanParser().Parse(xml).Single();
        var hint = Assert.Single(plan.EngineMissingIndexes);

        Assert.Empty(hint.EqualityColumns);
        Assert.Equal(new[] { "Total" }, hint.IncludeColumns);
        Assert.Empty(plan.Nodes.Single().PredicateColumns);
        Assert.Empty(plan.Nodes.Single().OutputColumns);
    }

    /// <summary>
    /// Tests that non-numeric and non-finite numeric attributes are treated as zero so they cannot poison cost ranking.
    /// </summary>
    [Theory]
    [InlineData("abc")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    public void NonFiniteOrInvalidNumbersFallBackToZero(string value)
    {
        var xml = Wrap($"""
            <StmtSimple StatementText="SELECT 1" StatementSubTreeCost="{value}">
              <QueryPlan>
                <RelOp PhysicalOp="Table Scan" EstimateRows="{value}" EstimatedTotalSubtreeCost="{value}" />
              </QueryPlan>
            </StmtSimple>
            """);

        var plan = new SqlServerXmlPlanParser().Parse(xml).Single();
        var node = plan.Nodes.Single();

        Assert.Equal(0, plan.EstimatedTotalCost);
        Assert.Equal(0, node.EstimatedRows);
        Assert.Equal(0, node.RelativeCost);
    }

    /// <summary>
    /// Tests that a statement whose plan has no operators parses into a plan with no nodes.
    /// </summary>
    [Fact]
    public void EmptyQueryPlanYieldsNoNodes()
    {
        var xml = Wrap("""
            <StmtSimple StatementText="SELECT 1" StatementSubTreeCost="0">
              <QueryPlan />
            </StmtSimple>
            """);

        var plan = new SqlServerXmlPlanParser().Parse(xml).Single();

        Assert.Empty(plan.Nodes);
        Assert.Empty(plan.EngineMissingIndexes);
    }

    /// <summary>
    /// Tests that a runtime counter without actual-row attributes contributes zero rows.
    /// </summary>
    [Fact]
    public void RunTimeCounterWithoutAttributesContributesZero()
    {
        var xml = Wrap("""
            <StmtSimple StatementText="SELECT 1" StatementSubTreeCost="2.0">
              <QueryPlan>
                <RelOp PhysicalOp="Table Scan" EstimateRows="5" EstimatedTotalSubtreeCost="1.0">
                  <RunTimeInformation>
                    <RunTimeCountersPerThread Thread="0" />
                  </RunTimeInformation>
                </RelOp>
              </QueryPlan>
            </StmtSimple>
            """);

        var node = new SqlServerXmlPlanParser().Parse(xml).Single().Nodes.Single();

        Assert.Equal(0, node.ActualRows);
        Assert.Equal(0, node.ActualRowsRead);
    }

    /// <summary>
    /// Tests that a document with no statements at all is parsed as a single empty plan.
    /// </summary>
    [Fact]
    public void DocumentWithoutStatementsYieldsSingleEmptyPlan()
    {
        const string xml = """
            <ShowPlanXML xmlns="http://schemas.microsoft.com/sqlserver/2004/07/showplan" />
            """;

        var plan = Assert.Single(new SqlServerXmlPlanParser().Parse(xml));

        Assert.Empty(plan.Nodes);
        Assert.Equal(string.Empty, plan.StatementText);
    }
}
