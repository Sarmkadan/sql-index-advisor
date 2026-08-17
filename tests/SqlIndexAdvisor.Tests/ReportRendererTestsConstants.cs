namespace SqlIndexAdvisor.Tests;

internal static class ReportRendererTestsConstants
{
    public const string DefaultTableName = "test_table";
    public const string UserTableName = "users";
    public const string OrdersTableName = "dbo.Orders";
    public const string ProductsTableName = "products";
    public const string MultipleRecommendationsTableName1 = "table1";
    public const string MultipleRecommendationsTableName2 = "table2";

    public const string DefaultOperator = "Seq Scan";
    
    public const double DefaultCost = 100.0;
    public const double LowCost = 50.0;
    public const double MediumCost = 150.0;
    public const double HighCost = 500.0;

    public const string NoRecommendationsMessage = "No index recommendations";
    public const string PlanLooksFineMessage = "The plan looks fine";
    public const string ImpactDisclaimerHeuristic = "Impact figures are rough heuristics";
    public const string ImpactDisclaimerValidation = "Validate before applying";
}
