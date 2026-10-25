namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Shared literals used by <see cref="SqlServerParserTestsExtensions"/>.
/// </summary>
internal static class SqlServerParserTestsExtensionsConstants
{
    /// <summary>
    /// XML envelope of a SQL Server showplan document.
    /// </summary>
    public const string XmlDeclaration = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
    public const string ShowPlanNamespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";
    public const string ShowPlanOpenTag = "<ShowPlanXML xmlns=\"" + ShowPlanNamespace + "\">";
    public const string ShowPlanCloseTag = "</ShowPlanXML>";
    public const string BatchSequenceOpenTag = "<BatchSequence><Batch><Statements>";
    public const string BatchSequenceCloseTag = "</Statements></Batch></BatchSequence>";

    /// <summary>
    /// Default statement and table naming.
    /// </summary>
    public const string DefaultSchemaName = "dbo";
    public const string OrdersTableName = "Orders";
    public const string StatusColumnName = "Status";
    public const string TotalColumnName = "Total";
    public const string IdColumnName = "Id";
    public const string PrimaryKeyPrefix = "PK_";
    public const string FromClausePrefix = "SELECT * FROM ";
    public const string OrdersStatementText = "SELECT * FROM dbo.Orders WHERE Status='open'";

    /// <summary>
    /// Physical operator and column usage names.
    /// </summary>
    public const string ClusteredIndexScanOperator = "Clustered Index Scan";
    public const string EqualityUsage = "EQUALITY";
    public const string IncludeUsage = "INCLUDE";

    /// <summary>
    /// Cost and row estimates.
    /// </summary>
    public const string DefaultSubtreeCost = "2.5";
    public const string OrdersTableEstimateRows = "1000";
    public const string IndexedTableEstimateRows = "500";

    /// <summary>
    /// Missing index impact bounds (percent).
    /// </summary>
    public const int DefaultImpactPercent = 80;
    public const int MinImpactPercent = 0;
    public const int MaxImpactPercent = 100;

    /// <summary>
    /// XML fragment templates (<c>{0}</c> is the column name).
    /// </summary>
    public const string ColumnElementTemplate = "<Column Name=\"[{0}]\" />";

    /// <summary>
    /// Argument validation messages.
    /// </summary>
    public const string NegativeCostMessage = "Cost must be non-negative";
    public const string ImpactPercentRangeMessage = "Impact percent must be between 0 and 100";
    public const string NoNodesMessage = "Execution plan has no nodes";
}
