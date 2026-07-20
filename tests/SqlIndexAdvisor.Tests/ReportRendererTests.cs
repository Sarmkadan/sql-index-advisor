using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Reporting;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Tests for the ReportRenderer class covering text and JSON rendering scenarios.
/// </summary>
public class ReportRendererTests
{
    /// <summary>
    /// Verifies that empty recommendation list produces appropriate text output.
    /// </summary>
    [Fact]
    public void RenderText_EmptyRecommendations_ReturnsNoRecommendationsMessage()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan", TableName = "users" } }
        };

        var recs = new List<IndexRecommendation>();
        var output = ReportRenderer.RenderText(plan, recs);

        Assert.Contains("No index recommendations", output);
        Assert.Contains("The plan looks fine", output);
        Assert.DoesNotContain("recommendation(s)", output);
    }

    /// <summary>
    /// Verifies that single recommendation includes table name in output.
    /// </summary>
    [Fact]
    public void RenderText_SingleRecommendation_IncludesTableName()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan", TableName = "dbo.Orders" } }
        };

        var recs = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "dbo.Orders",
                KeyColumns = new() { "Status" },
                EstimatedImpactPercent = 85.5,
                Confidence = Confidence.High,
                Reasons = { "Missing index on Status column" }
            }
        };

        var output = ReportRenderer.RenderText(plan, recs);

        Assert.Contains("1 recommendation(s)", output);
        Assert.Contains("dbo.Orders", output);
        Assert.Contains("85.5% estimated impact", output);
        Assert.Contains("High confidence", output);
        Assert.Contains("CREATE INDEX IX_Orders_Status ON dbo.Orders (Status);", output);
        Assert.Contains("- Missing index on Status column", output);
    }

    /// <summary>
    /// Verifies that single recommendation includes columns in the output.
    /// </summary>
    [Fact]
    public void RenderText_SingleRecommendation_IncludesColumns()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = 250.75,
            Nodes = { new PlanNode { Operator = "Seq Scan", TableName = "products" } }
        };

        var recs = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "products",
                KeyColumns = new() { "category_id", "price" },
                IncludeColumns = new() { "name", "description", "stock_quantity" },
                EstimatedImpactPercent = 72.3,
                Confidence = Confidence.Medium,
                Reasons = { "Frequent filtering on category_id and price", "Including non-key columns for covering index" }
            }
        };

        var output = ReportRenderer.RenderText(plan, recs);

        Assert.Contains("products", output);
        Assert.Contains("category_id", output);
        Assert.Contains("price", output);
        Assert.Contains("name", output);
        Assert.Contains("description", output);
        Assert.Contains("stock_quantity", output);
        Assert.Contains("72.3% estimated impact", output);
        Assert.Contains("Medium confidence", output);
    }

    /// <summary>
    /// Verifies that multiple recommendations are all present in output.
    /// </summary>
    [Fact]
    public void RenderText_MultipleRecommendations_AllPresent()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = 500,
            Nodes =
            {
                new PlanNode { Operator = "Seq Scan", TableName = "users" },
                new PlanNode { Operator = "Seq Scan", TableName = "orders" }
            }
        };

        var recs = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "users",
                KeyColumns = new() { "email" },
                EstimatedImpactPercent = 65.2,
                Confidence = Confidence.High,
                Reasons = { "High selectivity on email" }
            },
            new IndexRecommendation
            {
                Table = "orders",
                KeyColumns = new() { "customer_id", "order_date" },
                EstimatedImpactPercent = 45.8,
                Confidence = Confidence.Medium,
                Reasons = { "Frequent joins on customer_id", "Range queries on order_date" }
            }
        };

        var output = ReportRenderer.RenderText(plan, recs);

        Assert.Contains("2 recommendation(s)", output);
        Assert.Contains("[1]", output);
        Assert.Contains("[2]", output);
        Assert.Contains("users", output);
        Assert.Contains("orders", output);
        Assert.Contains("65.2% estimated impact", output);
        Assert.Contains("45.8% estimated impact", output);
    }

    /// <summary>
    /// Verifies that include columns render distinctly from key columns in text output.
    /// </summary>
    [Fact]
    public void RenderText_IncludeColumnsDistinctFromKeyColumns()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = 150,
            Nodes = { new PlanNode { Operator = "Index Scan", TableName = "dbo.CustomerOrders" } }
        };

        var recs = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "dbo.CustomerOrders",
                KeyColumns = new() { "CustomerID", "OrderDate" },
                IncludeColumns = new() { "TotalAmount", "Status", "CustomerName" },
                EstimatedImpactPercent = 90.1,
                Confidence = Confidence.High,
                Reasons = { "Covering index for common query pattern" }
            }
        };

        var output = ReportRenderer.RenderText(plan, recs);

        // Verify key columns appear in CREATE statement
        Assert.Contains("CREATE INDEX IX_CustomerOrders_CustomerID_OrderDate ON dbo.CustomerOrders (CustomerID, OrderDate)", output);

        // Verify include columns appear in INCLUDE clause
        Assert.Contains("INCLUDE (TotalAmount, Status, CustomerName)", output);

        // Verify both key and include columns are mentioned in reasons
        Assert.Contains("- Covering index for common query pattern", output);
    }

    /// <summary>
    /// Verifies that plan metadata is included in text output.
    /// </summary>
    [Fact]
    public void RenderText_IncludesPlanMetadata()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = 123.456,
            Nodes = { new PlanNode { Operator = "Seq Scan", TableName = "test_table" } }
        };

        var recs = new List<IndexRecommendation>();
        var output = ReportRenderer.RenderText(plan, recs);

        Assert.Contains("Dialect", output);
        Assert.Contains("Postgres", output);
        Assert.Contains("Statement cost", output);
        Assert.Contains("123.456", output);
        Assert.Contains("Operators", output);
        Assert.Contains("1", output);
    }

    /// <summary>
    /// Verifies that JSON output includes all required fields for empty recommendations.
    /// </summary>
    [Fact]
    public void RenderJson_EmptyRecommendations_IncludesAllFields()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = 50,
            Nodes = { new PlanNode { Operator = "Seq Scan", TableName = "test" } }
        };

        var recs = new List<IndexRecommendation>();
        var output = ReportRenderer.RenderJson(plan, recs);

        Assert.Contains("dialect", output);
        Assert.Contains("SqlServer", output);
        Assert.Contains("estimatedTotalCost", output);
        Assert.Contains("50", output);
        Assert.Contains("operatorCount", output);
        Assert.Contains("1", output);
        Assert.Contains("recommendations", output);
        Assert.Contains("[]", output);
    }

    /// <summary>
    /// Verifies that JSON output includes all required fields for single recommendation.
    /// </summary>
    [Fact]
    public void RenderJson_SingleRecommendation_IncludesAllRequiredFields()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = 200.5,
            Nodes = { new PlanNode { Operator = "Seq Scan", TableName = "test_table" } }
        };

        var recs = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "test_table",
                KeyColumns = new() { "col1", "col2" },
                IncludeColumns = new() { "col3", "col4" },
                EstimatedImpactPercent = 75.5,
                Confidence = Confidence.High,
                Reasons = { "Reason 1", "Reason 2" }
            }
        };

        var output = ReportRenderer.RenderJson(plan, recs);

        // Verify plan metadata
        Assert.Contains("dialect", output);
        Assert.Contains("Postgres", output);
        Assert.Contains("estimatedTotalCost", output);
        Assert.Contains("200.5", output);
        Assert.Contains("operatorCount", output);
        Assert.Contains("1", output);

        // Verify recommendation fields
        Assert.Contains("table", output);
        Assert.Contains("test_table", output);
        Assert.Contains("keyColumns", output);
        Assert.Contains("col1", output);
        Assert.Contains("col2", output);
        Assert.Contains("includeColumns", output);
        Assert.Contains("col3", output);
        Assert.Contains("col4", output);
        Assert.Contains("estimatedImpactPercent", output);
        Assert.Contains("75.5", output);
        Assert.Contains("confidence", output);
        Assert.Contains("High", output);
        Assert.Contains("createStatement", output);
        Assert.Contains("CREATE INDEX IX_test_table_col1_col2 ON test_table (col1, col2) INCLUDE (col3, col4);", output);
        Assert.Contains("reasons", output);
        Assert.Contains("Reason 1", output);
        Assert.Contains("Reason 2", output);
    }

    /// <summary>
    /// Verifies that JSON output includes all required fields for multiple recommendations.
    /// </summary>
    [Fact]
    public void RenderJson_MultipleRecommendations_AllPresent()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = 300,
            Nodes = { new PlanNode { Operator = "Seq Scan", TableName = "table1" } }
        };

        var recs = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "table1",
                KeyColumns = new() { "id" },
                EstimatedImpactPercent = 50,
                Confidence = Confidence.Medium,
                Reasons = { "Reason A" }
            },
            new IndexRecommendation
            {
                Table = "table2",
                KeyColumns = new() { "a", "b" },
                IncludeColumns = new() { "c" },
                EstimatedImpactPercent = 30,
                Confidence = Confidence.Low,
                Reasons = { "Reason B" }
            }
        };

        var output = ReportRenderer.RenderJson(plan, recs);

        Assert.Contains("recommendations", output);
        Assert.Contains("table1", output);
        Assert.Contains("table2", output);
        Assert.Contains("keyColumns", output);
        Assert.Contains("id", output);
        Assert.Contains("a", output);
        Assert.Contains("b", output);
        Assert.Contains("includeColumns", output);
        Assert.Contains("[]", output); // First rec has no includes
        Assert.Contains("c", output); // Second rec has includes
    }

    /// <summary>
    /// Verifies that JSON output is properly formatted and parseable.
    /// </summary>
    [Fact]
    public void RenderJson_OutputIsValidJson()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan", TableName = "test" } }
        };

        var recs = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "test",
                KeyColumns = new() { "col" },
                EstimatedImpactPercent = 50,
                Confidence = Confidence.High,
                Reasons = { "Test reason" }
            }
        };

        var output = ReportRenderer.RenderJson(plan, recs);

        // Should be valid JSON (basic check)
        Assert.StartsWith("{", output);
        Assert.Contains("dialect", output);
        Assert.Contains("recommendations", output);
        Assert.Contains(':', output);
        Assert.Contains(',', output);
    }

    /// <summary>
    /// Verifies that include columns are distinctly represented from key columns in JSON.
    /// </summary>
    [Fact]
    public void RenderJson_IncludeColumnsDistinctFromKeyColumns()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.SqlServer,
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan", TableName = "test" } }
        };

        var recs = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "test_table",
                KeyColumns = new() { "key_col1", "key_col2" },
                IncludeColumns = new() { "inc_col1", "inc_col2" },
                EstimatedImpactPercent = 80,
                Confidence = Confidence.High,
                Reasons = { "Test" }
            }
        };

        var output = ReportRenderer.RenderJson(plan, recs);

        // Verify key columns array
        Assert.Contains("keyColumns", output);
        Assert.Contains("key_col1", output);
        Assert.Contains("key_col2", output);

        // Verify include columns array (distinct from key columns)
        Assert.Contains("includeColumns", output);
        Assert.Contains("inc_col1", output);
        Assert.Contains("inc_col2", output);

        // Both should be present
        var keyIndex = output.IndexOf("keyColumns", StringComparison.Ordinal);
        var includeIndex = output.IndexOf("includeColumns", StringComparison.Ordinal);
        Assert.True(keyIndex >= 0);
        Assert.True(includeIndex >= 0);
    }

    /// <summary>
    /// Verifies that text output includes impact disclaimer.
    /// </summary>
    [Fact]
    public void RenderText_IncludesImpactDisclaimer()
    {
        var plan = new ExecutionPlan
        {
            Dialect = PlanDialect.Postgres,
            EstimatedTotalCost = 100,
            Nodes = { new PlanNode { Operator = "Seq Scan", TableName = "test" } }
        };

        var recs = new List<IndexRecommendation>
        {
            new IndexRecommendation
            {
                Table = "test",
                KeyColumns = new() { "col" },
                EstimatedImpactPercent = 50,
                Confidence = Confidence.High,
                Reasons = { }
            }
        };

        var output = ReportRenderer.RenderText(plan, recs);
        Assert.Contains("Impact figures are rough heuristics", output);
        Assert.Contains("Validate before applying", output);
    }
}