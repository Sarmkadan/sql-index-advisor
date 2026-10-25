namespace SqlIndexAdvisor.Tests
{
    internal static class ExecutionPlanTestsConstants
    {
        // Table names
        public const string UsersTable = "dbo.Users";
        public const string OrdersTable = "dbo.Orders";
        public const string TestTable = "dbo.TestTable";
        public const string Table1 = "Table1";
        public const string Table2 = "Table2";

        // Index names
        public const string PrimaryKeyUsersIndex = "PK_Users";

        // Column names
        public const string NameColumn = "Name";
        public const string EmailColumn = "Email";
        public const string CreatedDateColumn = "CreatedDate";
        public const string Col1Column = "Col1";
        public const string Col2Column = "Col2";
        public const string Col3Column = "Col3";
        public const string Col4Column = "Col4";
        public const string Col5Column = "Col5";
        public const string Col6Column = "Col6";

        // Operator names
        public const string ClusteredIndexScanOperator = "Clustered Index Scan";
        public const string IndexScanOperator = "Index Scan";
        public const string IndexOnlyScanOperator = "Index Only Scan";
        public const string FilterOperator = "Filter";
        public const string ParentOperator = "Parent";
        public const string ChildOperator = "Child";

        // Statement text
        public const string UsersByNameStatement = "SELECT * FROM Users WHERE Name = 'test'";
        public const string ProductsByPriceStatement = "SELECT * FROM Products WHERE Price > 100";

        // Sample plan values
        public const double SamplePlanEstimatedTotalCost = 123.45;
        public const int SamplePlanUsersEstimatedRows = 1000;
        public const int SamplePlanOrdersEstimatedRows = 500;
        public const int SamplePlanNodeCount = 2;
        public const int SamplePlanEngineMissingIndexCount = 1;
        public const double SamplePlanImpactPercent = 95.5;

        // Other numeric values
        public const int TwoItemCollectionCount = 2;
        public const double MaxEstimatedTotalCost = 999.99;
        public const double NegativeEstimatedTotalCost = -1.5;
        public const double MissingIndexImpactPercent = 50;
        public const double OtherMissingIndexImpactPercent = 75;
        public const double TestTableImpactPercent = 87.3;
        public const int TestTableIncludeColumnCount = 3;
        public const int TestTableEqualityColumnCount = 2;
    }
}
