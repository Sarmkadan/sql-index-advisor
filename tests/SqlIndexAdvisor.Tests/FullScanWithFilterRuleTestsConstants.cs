namespace SqlIndexAdvisor.Tests
{
    internal static class FullScanWithFilterRuleTestsConstants
    {
        // Table names
        public const string UsersTable = "users";
        public const string OrdersTable = "orders";
        public const string ProductsTable = "products";
        public const string CustomersTable = "customers";
        public const string LogsTable = "logs";
        public const string AuditTable = "audit";

        // Common numeric values
        public const int EstimatedTotalCost = 100;

        // Relative cost thresholds
        public const double MinRelativeCost = 0.10;
        public const double JustBelowMinRelativeCost = 0.099;
        public const double JustAboveMinRelativeCost = 0.101;

        public const double HighRelativeCost = 0.9;
        public const double MediumHighRelativeCost = 0.8;
        public const double LowRelativeCost = 0.05;
        public const double MediumRelativeCost = 0.5;
        public const double LowMediumRelativeCost = 0.3;
        public const double LowRelativeCost2 = 0.2;
        public const double MediumLowRelativeCost = 0.4;

        // Expected reason strings
        public const string SeqScanUsersReason = "Seq Scan on users carries a filter on (id) and is ~90% of statement cost.";
        public const string ClusteredIndexScanOrdersReason = "Clustered Index Scan on orders carries a filter on (status) and is ~80% of statement cost.";
        public const string IndexScanProductsReason = "Index Scan on products carries a filter on (category_id, price) and is ~50% of statement cost.";
        public const string SeqScanLogsReason = "Seq Scan on logs carries a filter on (timestamp) and is ~90% of statement cost.";

        // Rule name
        public const string RuleName = "fullscanwithfilter";
    }
}
