using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    public interface IIndexRecommendationTests
    {
        void Constructor_WithRequiredProperties_InitializesCorrectly();
        void Constructor_WithEmptyIncludeColumns_InitializesCorrectly();
        void SuggestedName_WithValidTableAndColumns_ReturnsCorrectFormat();
        void SuggestedName_WithSchemaQualifiedTable_ReturnsCorrectFormat();
        void SuggestedName_WithSpecialCharactersInTableName_SanitizesCorrectly();
        void SuggestedName_WithSpecialCharactersInColumns_SanitizesCorrectly();
        void SuggestedName_WithSingleColumn_ReturnsCorrectFormat();
        void ToCreateStatement_WithKeyAndIncludeColumns_ReturnsCorrectSql();
        void ToCreateStatement_WithOnlyKeyColumns_ReturnsCorrectSql();
        void ToCreateStatement_WithOnlyIncludeColumns_ReturnsCorrectSql();
        void ToCreateStatement_WithEmptyTableName_ReturnsStatementWithEmptyTable();
        void ToCreateStatement_WithNullKeyColumns_ThrowsArgumentNullException();
        void ToCreateStatement_WithNullIncludeColumns_ThrowsNullReferenceException();
        void ToCreateStatement_WithMultipleKeyColumns_ReturnsCorrectSql();
        void SuggestedName_WithSchemaInTableName_ExtractsTableNameCorrectly();
        void SuggestedName_WithNumbersInTableName_ReturnsCorrectFormat();
        void ToCreateStatement_WithNullDialect_DoesNotThrow();
        void EstimatedImpactPercent_WithBoundaryValues_StoresCorrectly();
        void EstimatedImpactPercent_WithMaximumValue_StoresCorrectly();
        void SourceNodeCost_WithValidValue_StoresCorrectly();
    }
}
