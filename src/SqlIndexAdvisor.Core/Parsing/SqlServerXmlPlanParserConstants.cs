namespace SqlIndexAdvisor.Core.Parsing;

internal static class SqlServerXmlPlanParserConstants
{
    public const string ShowPlanXmlMarker = "ShowPlanXML";
    public const string StmtSimpleElement = "StmtSimple";
    public const string StatementTextAttribute = "StatementText";
    public const string StatementSubTreeCostAttribute = "StatementSubTreeCost";
    public const string RelOpElement = "RelOp";
    public const string EstimateRowsAttribute = "EstimateRows";
    public const string PhysicalOpAttribute = "PhysicalOp";
    public const string LogicalOpAttribute = "LogicalOp";
    public const string UnknownOperator = "Unknown";
    public const string EstimatedTotalSubtreeCostAttribute = "EstimatedTotalSubtreeCost";
    public const string MissingIndexGroupElement = "MissingIndexGroup";
    public const string ImpactAttribute = "Impact";
    public const string MissingIndexElement = "MissingIndex";
    public const string TableAttribute = "Table";
    public const string SchemaAttribute = "Schema";
    public const string ColumnGroupElement = "ColumnGroup";
    public const string UsageAttribute = "Usage";
    public const string ColumnElement = "Column";
    public const string NameAttribute = "Name";
    public const string ObjectElement = "Object";
    public const string IndexAttribute = "Index";
    public const string PredicateElement = "Predicate";
    public const string ColumnReferenceElement = "ColumnReference";
    public const string OutputListElement = "OutputList";
    public const string EqualityUsage = "EQUALITY";
    public const string InequalityUsage = "INEQUALITY";
    public const string BracketOpen = "[";
    public const string BracketClose = "]";
    public const string SchemaTableSeparator = ".";

    public const int MaxCharactersInDocument = 10_000_000;
    public const int MaxCharactersFromEntities = 1_000_000;

    public const string ParseFailedMessageFormat =
        "Failed to parse SQL Server execution plan XML at line {0}, position {1}. "
        + "The file may be a saved .sqlplan wrapper rather than raw ShowPlanXML. "
        + "Verify the file contains valid SQL Server showplan XML.";
    public const string NotWellFormedXmlMessage = "Content is not well-formed XML.";
}