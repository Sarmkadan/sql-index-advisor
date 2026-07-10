using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

public class PostgresParserTests
{
    private const string SeqScanPlan = """
    [{"Plan":{"Node Type":"Seq Scan","Relation Name":"users","Total Cost":11822.55,
      "Plan Rows":88,"Output":["id","email"],
      "Filter":"((country = 'PL'::text) AND (is_active = true))"}}]
    """;

    [Fact]
    public void DetectsFormat()
    {
        Assert.True(new PostgresJsonPlanParser().CanParse(SeqScanPlan));
        Assert.False(new SqlServerXmlPlanParser().CanParse(SeqScanPlan));
    }

    [Fact]
    public void ParsesSeqScanFilterColumns()
    {
        var plan = new PostgresJsonPlanParser().Parse(SeqScanPlan);
        Assert.Equal(PlanDialect.Postgres, plan.Dialect);
        var scan = plan.Nodes.Single(n => n.Operator == "Seq Scan");
        Assert.Equal("users", scan.TableName);
        Assert.Contains("country", scan.PredicateColumns);
        Assert.Contains("is_active", scan.PredicateColumns);
    }

    [Fact]
    public void EmptyArrayThrows()
    {
        Assert.Throws<PlanParseException>(() => new PostgresJsonPlanParser().Parse("[]"));
    }

    [Fact]
    public void GarbageThrows()
    {
        Assert.Throws<PlanParseException>(() => new PostgresJsonPlanParser().Parse("{not json"));
    }
}
