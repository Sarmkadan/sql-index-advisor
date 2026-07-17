using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Parsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Contains unit tests for parsing PostgreSQL execution plans using the <see cref="PostgresJsonPlanParser"/> class.
/// These tests verify that the parser correctly identifies PostgreSQL plan format and extracts relevant information
/// such as table names, predicate columns, and plan properties from JSON-formatted execution plans.
/// </summary>
public class PostgresParserTests
{
	private const string SeqScanPlan = """
[{"Plan":{"Node Type":"Seq Scan","Relation Name":"users","Total Cost":11822.55,
"Plan Rows":88,"Output":["id","email"],
"Filter":"((country = 'PL'::text) AND (is_active = true))"}}]
""";

	/// <summary>
	/// Tests that the parser correctly identifies PostgreSQL JSON plan format and distinguishes it from SQL Server XML format.
	/// Verifies that <see cref="PostgresJsonPlanParser"/> returns true for PostgreSQL plans and false for SQL Server plans.
	/// </summary>
	[Fact]
	public void DetectsFormat()
	{
		Assert.True(new PostgresJsonPlanParser().CanParse(SeqScanPlan));
		Assert.False(new SqlServerXmlPlanParser().CanParse(SeqScanPlan));
	}

	/// <summary>
	/// Tests parsing of a PostgreSQL sequential scan execution plan to verify correct extraction of plan properties.
	/// Validates that the parser correctly identifies the plan dialect, table name, and predicate columns
	/// from a JSON-formatted PostgreSQL execution plan.
	/// </summary>
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

	/// <summary>
	/// Tests that parsing an empty JSON array throws a <see cref="PlanParseException"/>.
	/// Verifies proper error handling when attempting to parse invalid PostgreSQL plan data.
	/// </summary>
	[Fact]
	public void EmptyArrayThrows()
	{
		Assert.Throws<PlanParseException>(() => new PostgresJsonPlanParser().Parse("[]"));
	}

	/// <summary>
	/// Tests that parsing malformed JSON input throws a <see cref="PlanParseException"/>.
	/// Verifies proper error handling when attempting to parse invalid JSON data as a PostgreSQL execution plan.
	/// </summary>
	[Fact]
	public void GarbageThrows()
	{
		Assert.Throws<PlanParseException>(() => new PostgresJsonPlanParser().Parse("{not json"));
	}
}