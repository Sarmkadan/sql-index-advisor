namespace SqlIndexAdvisor.Tests;

internal static class PostgresParserTestsConstants
{
    public const string SeqScanPlan = """
[{"Plan":{"Node Type":"Seq Scan","Relation Name":"users","Total Cost":11822.55,
"Plan Rows":88,"Output":["id","email"],
"Filter":"((country = 'PL'::text) AND (is_active = true))"}}]
""";
    public const string SeqScanOperator = "Seq Scan";
    public const string UsersTableName = "users";
    public const string CountryColumn = "country";
    public const string IsActiveColumn = "is_active";
    public const string EmptyJsonArray = "[]";
    public const string InvalidJson = "{not json";
}
