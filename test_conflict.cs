using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Rules;

// Test to demonstrate the conflict between EngineHintRule and FullScanWithFilterRule
var plan = new ExecutionPlan
{
    Dialect = PlanDialect.SqlServer,
    StatementText = "SELECT * FROM Users WHERE Country = 'US' AND IsActive = 1",
    EstimatedTotalCost = 100.0,
    EngineMissingIndexes = new List<EngineMissingIndex>
    {
        new EngineMissingIndex
        {
            Table = "Users",
            ImpactPercent = 95.3,
            EqualityColumns = new List<string> { "Country", "IsActive" },
            IncludeColumns = new List<string> { "Name", "Email" }
        }
    },
    Nodes = new List<PlanNode>
    {
        new PlanNode
        {
            Operator = "Table Scan",
            TableName = "Users",
            RelativeCost = 0.85,
            PredicateColumns = new List<string> { "Country", "IsActive" },
            OutputColumns = new List<string> { "Id", "Name", "Email", "Country", "IsActive" }
        }
    }
};

var engine = new RecommendationEngine();
var recommendations = engine.Analyze(plan);

Console.WriteLine("Recommendations generated:");
foreach (var rec in recommendations)
{
    Console.WriteLine($"\nTable: {rec.Table}");
    Console.WriteLine($"Key Columns: {string.Join(", ", rec.KeyColumns)}");
    Console.WriteLine($"Impact: {rec.EstimatedImpactPercent}%");
    Console.WriteLine($"Rule: {rec.Rule}");
    Console.WriteLine("Reasons:");
    foreach (var reason in rec.Reasons)
    {
        Console.WriteLine($"  - {reason}");
    }
}
