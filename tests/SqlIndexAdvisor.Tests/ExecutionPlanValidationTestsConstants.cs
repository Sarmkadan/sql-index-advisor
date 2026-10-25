namespace SqlIndexAdvisor.Tests
{
    internal static class ExecutionPlanValidationTestsConstants
    {
        // Statements
        public const string SelectAllUsersStatement = "SELECT * FROM users";
        public const string PostgresActiveUsersStatement = "SELECT * FROM users WHERE active = true";

        // Table names
        public const string UsersTable = "users";

        // Operator names
        public const string SeqScanOperator = "Seq Scan";
        public const string IndexScanOperator = "Index Scan";

        // Column names
        public const string CountryColumn = "country";
        public const string IsActiveColumn = "is_active";
        public const string IdColumn = "id";
        public const string EmailColumn = "email";
        public const string NameColumn = "name";
        public const string ActiveColumn = "active";

        // Plan values
        public const double ValidEstimatedTotalCost = 100;
        public const double NegativeEstimatedTotalCost = -100;
        public const double NegativeEstimatedTotalCostAlt = -50;
        public const double SeqScanEstimatedRows = 50;
        public const double SeqScanEstimatedRowsRead = 500000;
        public const double SeqScanRelativeCost = 0.9;
        public const double IndexScanEstimatedRows = 10;
        public const double IndexScanEstimatedRowsRead = 10;
        public const double IndexScanRelativeCost = 0.1;
        public const double PostgresEstimatedTotalCost = 45.5;
        public const double PostgresEstimatedRows = 1000;
        public const double PostgresEstimatedRowsRead = 1000;
        public const double PostgresRelativeCost = 0.85;
        public const double NegativeEstimatedRows = -1;
        public const double OutOfRangeRelativeCost = 1.5;
        public const double OutOfRangeImpactPercent = 150;
        public const int InvalidDialectValue = 999;

        // Expected validation messages
        public const string InvalidDialectMessage = "Invalid Dialect value";
        public const string EstimatedTotalCostNaNMessage = "EstimatedTotalCost cannot be NaN";
        public const string EstimatedTotalCostInfiniteMessage = "EstimatedTotalCost cannot be infinite";
        public const string NodesNullMessage = "Nodes collection cannot be null";
        public const string NullNodeMessage = "Nodes[0] cannot be null";
        public const string EmptyOperatorMessage = "Nodes[0].Operator cannot be null or empty";
        public const string NegativeEstimatedRowsMessage = "Nodes[0].EstimatedRows cannot be negative";
        public const string NaNEstimatedRowsMessage = "Nodes[0].EstimatedRows cannot be NaN";
        public const string OutOfRangeRelativeCostMessage = "Nodes[0].RelativeCost must be between 0 and 1";
        public const string NullPredicateColumnsMessage = "Nodes[0].PredicateColumns cannot be null";
        public const string NullEngineMissingIndexesMessage = "EngineMissingIndexes collection cannot be null";
        public const string NullMissingIndexMessage = "EngineMissingIndexes[0] cannot be null";
        public const string EmptyMissingIndexTableMessage = "EngineMissingIndexes[0].Table cannot be null or empty";
        public const string OutOfRangeImpactPercentMessage = "EngineMissingIndexes[0].ImpactPercent must be between 0 and 100";
    }
}
