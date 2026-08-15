using System;

namespace SqlIndexAdvisor.Tests
{
    public interface IExecutionPlanTests
    {
        void Constructor_WithDefaultValues_CreatesEmptyCollections();
        void Dialect_SetAndGet_ReturnsCorrectValue();
        void Dialect_WithSqlServerValue_ReturnsSqlServer();
        void Dialect_WithPostgresValue_ReturnsPostgres();
        void StatementText_SetAndGet_ReturnsCorrectValue();
        void StatementText_WithEmptyString_ReturnsEmptyString();
        void StatementText_WithNullValue_DefaultIsEmptyString();
        void EstimatedTotalCost_SetAndGet_ReturnsCorrectValue();
        void EstimatedTotalCost_WithZeroValue_ReturnsZero();
        void EstimatedTotalCost_WithNegativeValue_ReturnsNegativeValue();
        void Nodes_SetAndGet_ReturnsCorrectCollection();
        void Nodes_WithEmptyList_ReturnsEmptyCollection();
        void Nodes_WithNullValue_DefaultIsEmptyList();
        void EngineMissingIndexes_SetAndGet_ReturnsCorrectCollection();
        void EngineMissingIndexes_WithEmptyList_ReturnsEmptyCollection();
        void EngineMissingIndexes_WithNullValue_DefaultIsEmptyList();
        void SamplePlan_HasCorrectDialect();
        void SamplePlan_HasCorrectStatementText();
        void SamplePlan_HasCorrectEstimatedTotalCost();
        void SamplePlan_HasCorrectNodesCount();
    }
}
