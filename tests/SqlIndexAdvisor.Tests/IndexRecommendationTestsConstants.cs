namespace SqlIndexAdvisor.Tests;

public static class IndexRecommendationTestsConstants
{
    public const string TestTableUsers = "dbo.Users";
    public const string TestColumnUserId = "UserId";
    public const string TestColumnEmail = "Email";
    public const string TestColumnName = "Name";
    public const string TestColumnCreatedDate = "CreatedDate";
    public const double TestImpactPercentDefault = 85.5;
    public const double TestSourceNodeCostDefault = 0.75;

    public const string TestTableProducts = "dbo.Products";
    public const string TestColumnProductId = "ProductId";
    public const string TestColumnProductName = "ProductName";
    public const string TestColumnPrice = "Price";
    public const double TestImpactPercentProducts = 90.0;
    public const double TestSourceNodeCostProducts = 0.80;

    public const string TestTableOrders = "dbo.Orders";
    public const string TestColumnOrderId = "OrderId";
    public const string TestColumnTotalAmount = "TotalAmount";
}
