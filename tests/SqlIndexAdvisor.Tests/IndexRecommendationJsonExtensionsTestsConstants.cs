namespace SqlIndexAdvisor.Tests
{
    internal static class IndexRecommendationJsonExtensionsTestsConstants
    {
        public const string TableUsers = "dbo.Users";
        public const string TableProducts = "dbo.Products";
        public const string TableOrders = "dbo.Orders";
        public const string TableMinimal = "dbo.Minimal";
        public const string TableTest = "dbo.Test";

        public const string ColumnUserId = "UserId";
        public const string ColumnEmail = "Email";
        public const string ColumnName = "Name";
        public const string ColumnCreatedDate = "CreatedDate";
        public const string ColumnProductId = "ProductId";
        public const string ColumnCategoryId = "CategoryId";
        public const string ColumnProductName = "ProductName";
        public const string ColumnOrderId = "OrderId";
        public const string ColumnId = "Id";

        public const double EstimatedImpactPercent85_5 = 85.5;
        public const double EstimatedImpactPercent72_3 = 72.3;
        public const double EstimatedImpactPercent45_2 = 45.2;
        public const double EstimatedImpactPercent0 = 0.0;
        public const double EstimatedImpactPercent50 = 50.0;

        public const double SourceNodeCost0_75 = 0.75;
        public const double SourceNodeCost0_5 = 0.5;
        public const double SourceNodeCost0_3 = 0.3;
        public const double SourceNodeCost0 = 0.0;

        public const string ReasonMissingIndexOnUsersTable = "Missing index on Users table";
        public const string ReasonFrequentWhereClause = "Frequent WHERE clause on UserId and Email";
        public const string ReasonMissingIndex = "Missing index";
        public const string ReasonPerformanceIssue = "Performance issue";

        public const string WhitespaceJson = "   ";
        public const string InvalidJson = "{ invalid json";
    }
}
