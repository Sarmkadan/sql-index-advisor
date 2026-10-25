using System.Globalization;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Regression tests that verify both plan parsers read numeric values independently of the current culture.
/// A comma-decimal culture such as <c>de-DE</c> must not change how costs or row counts are interpreted.
/// </summary>
public class ParserCultureTests
{
    private const string GermanCultureName = "de-DE";

    private const string SqlServerPlan = """
    <?xml version="1.0" encoding="utf-16"?>
    <ShowPlanXML xmlns="http://schemas.microsoft.com/sqlserver/2004/07/showplan">
      <BatchSequence><Batch><Statements>
        <StmtSimple StatementText="SELECT * FROM dbo.Orders" StatementSubTreeCost="4.5">
          <QueryPlan>
            <RelOp PhysicalOp="Clustered Index Scan" EstimateRows="1234.5" EstimatedTotalSubtreeCost="4.5">
              <IndexScan>
                <Object Schema="[dbo]" Table="[Orders]" Index="[PK_Orders]" />
              </IndexScan>
            </RelOp>
          </QueryPlan>
        </StmtSimple>
      </Statements></Batch></BatchSequence>
    </ShowPlanXML>
    """;

    private const string PostgresPlan = """
    [
      {
        "Plan": {
          "Node Type": "Seq Scan",
          "Relation Name": "users",
          "Total Cost": 4.5,
          "Plan Rows": 1234.5
        }
      }
    ]
    """;

    /// <summary>
    /// Tests that the SQL Server parser reads cost and row attributes as invariant numbers under <c>de-DE</c>,
    /// where a naive parse would treat <c>4.5</c> as <c>45</c>.
    /// </summary>
    [Fact]
    public void SqlServerParserReadsInvariantNumbersUnderGermanCulture()
    {
        var plan = WithCulture(GermanCultureName, () => new SqlServerXmlPlanParser().Parse(SqlServerPlan).Single());

        Assert.Equal(4.5, plan.EstimatedTotalCost, 3);
        var scan = Assert.Single(plan.Nodes);
        Assert.Equal(1234.5, scan.EstimatedRows, 3);
    }

    /// <summary>
    /// Tests that the PostgreSQL parser reads cost and row properties as invariant numbers under <c>de-DE</c>.
    /// </summary>
    [Fact]
    public void PostgresParserReadsInvariantNumbersUnderGermanCulture()
    {
        var plan = WithCulture(GermanCultureName, () => new PostgresJsonPlanParser().Parse(PostgresPlan).Single());

        Assert.Equal(4.5, plan.EstimatedTotalCost, 3);
        var scan = Assert.Single(plan.Nodes);
        Assert.Equal(1234.5, scan.EstimatedRows, 3);
    }

    /// <summary>
    /// Runs <paramref name="action"/> with the given current culture and restores the previous culture afterwards.
    /// </summary>
    /// <typeparam name="T">The result type of the action.</typeparam>
    /// <param name="cultureName">The culture to apply, such as <c>de-DE</c>.</param>
    /// <param name="action">The work to run under the culture.</param>
    /// <returns>The value returned by <paramref name="action"/>.</returns>
    private static T WithCulture<T>(string cultureName, Func<T> action)
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo(cultureName);
        try
        {
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
