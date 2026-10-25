namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Shared literals for the RecommendationMerger tests.
/// </summary>
internal static class RecommendationMergerTestsConstants
{
    // Tables
    public const string UsersTable = "Users";
    public const string OrdersTable = "Orders";
    public const string ProductsTable = "Products";
    public const string SalesTable = "Sales";

    // Columns
    public const string CountryColumn = "Country";
    public const string IsActiveColumn = "IsActive";
    public const string CreatedAtColumn = "CreatedAt";
    public const string NameColumn = "Name";
    public const string EmailColumn = "Email";
    public const string TotalOrdersColumn = "TotalOrders";
    public const string StatusColumn = "Status";
    public const string OrderDateColumn = "OrderDate";
    public const string TotalColumn = "Total";
    public const string CustomerIdColumn = "CustomerId";
    public const string IdColumn = "Id";
    public const string TotalAmountColumn = "TotalAmount";
    public const string QuantityColumn = "Quantity";
    public const string PriceColumn = "Price";
    public const string ProductIdColumn = "ProductId";
    public const string CategoryColumn = "Category";
    public const string DescriptionColumn = "Description";
    public const string StockQuantityColumn = "StockQuantity";
    public const string RegionColumn = "Region";
    public const string YearColumn = "Year";
    public const string MonthColumn = "Month";
    public const string CountColumn = "Count";
    public const string QuarterColumn = "Quarter";
    public const string DayColumn = "Day";

    // Reasons
    public const string Rule1Reason = "Rule1";
    public const string Rule2Reason = "Rule2";
    public const string Rule3Reason = "Rule3";
    public const string KeyLookupRuleReason = "KeyLookupRule";
    public const string FullScanWithFilterRuleReason = "FullScanWithFilterRule";
    public const string ExpensiveSortRuleReason = "ExpensiveSortRule";

    // Estimated impact percentages
    public const double LowImpactPercent = 30.0;
    public const double LowerImpactPercent = 40.0;
    public const double BaseImpactPercent = 50.0;
    public const double ModerateImpactPercent = 60.0;
    public const double HighImpactPercent = 65.0;
    public const double PeakImpactPercent = 75.0;
    public const double MaxImpactPercent = 80.0;

    // Expected counts
    public const int ExpectedUnmergedCount = 2;
    public const int ExpectedMergedReasonCount = 3;
    public const int ExpectedMergedIncludeCount = 4;
    public const int ExpectedMultiMergeIncludeCount = 3;
    // 3 original reasons + 2 heuristic notes from 2 merges
    public const int ExpectedMultiMergeReasonCount = 5;
}
