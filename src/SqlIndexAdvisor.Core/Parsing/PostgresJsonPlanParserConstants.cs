namespace SqlIndexAdvisor.Core.Parsing;

internal static class PostgresJsonPlanParserConstants
{
    public const string NodeTypePropertyName = "Node Type";
    public const string PlanPropertyName = "Plan";
    public const string QueryTextPropertyName = "Query Text";
    public const string TotalCostPropertyName = "Total Cost";
    public const string RelationNamePropertyName = "Relation Name";
    public const string IndexNamePropertyName = "Index Name";
    public const string PlanRowsPropertyName = "Plan Rows";
    public const string ActualRowsPropertyName = "Actual Rows";
    public const string ActualLoopsPropertyName = "Actual Loops";
    public const string RowsRemovedByFilterPropertyName = "Rows Removed by Filter";
    public const string RowsRemovedByIndexRecheckPropertyName = "Rows Removed by Index Recheck";
    public const string OutputPropertyName = "Output";
    public const string PlansPropertyName = "Plans";
    public const string FilterPropertyName = "Filter";
    public const string IndexCondPropertyName = "Index Cond";
    public const string RecheckCondPropertyName = "Recheck Cond";
    public const string HashCondPropertyName = "Hash Cond";
}
