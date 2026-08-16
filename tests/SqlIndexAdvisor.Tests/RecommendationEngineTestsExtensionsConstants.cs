namespace SqlIndexAdvisor.Tests
{
    internal static class RecommendationEngineTestsExtensionsConstants
    {
        // SQL statement fragments
        public const string CreateIndexPrefix = "CREATE INDEX ";
        public const string OnClause = " ON ";
        public const string IncludePrefix = "INCLUDE (";
        public const string ColumnSeparator = ", ";

        // Parenthesis characters
        public const char OpeningParen = '(';
        public const char ClosingParen = ')';

        // Default values for sequential scan plan
        public const int SeqScanEstimatedTotalCost = 100;
        public const int SeqScanEstimatedRows = 1000;
        public const int SeqScanEstimatedRowsRead = 1000000;
        public const double SeqScanRelativeCost = 0.9;
        public const string SeqScanPredicateColumn = "id";

        // Default values for clustered index scan plan
        public const int ClusteredScanEstimatedTotalCost = 10;
        public const int ClusteredScanEstimatedRows = 100;
        public const int ClusteredScanEstimatedRowsRead = 10000;
        public const double ClusteredScanRelativeCost = 0.8;
        public const string ClusteredScanPredicateColumn = "status";
        public static readonly string[] ClusteredScanOutputColumns = new[] { "id", "total", "customer_id" };
    }
}
