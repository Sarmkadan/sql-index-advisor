namespace SqlIndexAdvisor.Tests;

internal static class RecommendationEngineTestsConstants
{
    public const string SeqScanOperator = "Seq Scan";
    public const string ClusteredIndexScanOperator = "Clustered Index Scan";
    public const string UsersTable = "users";
    public const string TinyTable = "tiny";
    public const string OrdersTable = "dbo.Orders";
    
    public const string CountryColumn = "country";
    public const string IsActiveColumn = "is_active";
    public const string IdColumn = "id";
    public const string EmailColumn = "email";
    public const string StatusColumn = "Status";
    public const string TotalColumn = "Total";
    public const string CustomerIdColumn = "CustomerId";
    public const string CreatedAtColumn = "CreatedAt";
    public const string XColumn = "x";

    public const double DefaultRelativeCost = 0.9;
    public const double CheapRelativeCost = 0.02;
    public const double ClusteredIndexScanRelativeCost = 0.95;
    
    public const int DefaultEstimatedTotalCost = 100;
    public const int DefaultEstimatedRows = 50;
    public const int DefaultEstimatedRowsRead = 500000;
    
    public const int OrdersEstimatedTotalCost = 10;
    public const int OrdersImpactPercent = 80;
    public const int OrdersEstimatedRows = 10;
    public const int OrdersEstimatedRowsRead = 1000;
    
    public const string CreateIndexStatement = "CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders (Status, CreatedAt)";
    public const string IncludeStatement = "INCLUDE (Total)";
}
