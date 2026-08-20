namespace SqlIndexAdvisor.Tests;

internal static class IndexRecommendationExtensionsTestsConstants
{
    public const string TableUsers = "dbo.Users";
    public const string ColumnUserId = "UserId";
    public const string ColumnEmail = "Email";
    public const string ColumnName = "Name";
    public const string ColumnCreatedDate = "CreatedDate";
    public const double EstimatedImpactPercentHigh = 85.5;
    public const string ReasonMissingIndex = "Missing index on Users table";
    public const string ReasonFrequentWhereClause = "Frequent WHERE clause on UserId and Email";
    public const string TableProducts = "dbo.Products";
    public const string ColumnProductName = "ProductName";
    public const string ColumnProductId = "ProductId";
    public const string ColumnCategoryId = "CategoryId";
    public const string TableOrders = "dbo.Orders";
    public const string ColumnOrderDate = "OrderDate";
    public const string ColumnTotalAmount = "TotalAmount";
    public const double EstimatedImpactPercentMedium = 42.3;
    public const string TableEmptyTable = "dbo.EmptyTable";
    public const string ColumnOrderDateSingle = "OrderDate";
    public const string TableCustomers = "dbo.Customers";
    public const string ColumnCustomerId = "CustomerId";
    public const double EstimatedImpactPercentLow = 12.5;
    public const string ColumnCustomerName = "CustomerName";
    public const double EstimatedImpactPercentMediumLow = 37.8;
}
