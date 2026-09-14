using System.Collections.Generic;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Represents a recommendation for a database index.
/// </summary>
public interface IIndexRecommendation
{
    /// <summary>
    /// Gets or sets the name of the table for which the index is recommended.
    /// </summary>
    string Table { get; init; }

    /// <summary>
    /// Gets or sets the list of column names that should be key columns in the index.
    /// </summary>
    List<string> KeyColumns { get; init; }

    /// <summary>
    /// Gets or sets the list of column names that should be included as non-key columns in the index.
    /// </summary>
    List<string> IncludeColumns { get; init; }

    /// <summary>
    /// Gets or sets the estimated performance improvement percentage if this index is implemented.
    /// </summary>
    double EstimatedImpactPercent { get; init; }

    /// <summary>
    /// Gets or sets the cost of the source node in the query plan that this index would improve.
    /// </summary>
    double SourceNodeCost { get; init; }

    /// <summary>
    /// Gets or sets the confidence level in this recommendation.
    /// </summary>
    Confidence Confidence { get; init; }

    /// <summary>
    /// Gets or sets the list of reasons supporting this index recommendation.
    /// </summary>
    List<string> Reasons { get; init; }

    /// <summary>
    /// Generates a suggested name for the index.
    /// </summary>
    /// <param name="existingIndexName">The name of an existing index to consider when generating a suggestion, or null.</param>
    /// <returns>A suggested name for the index.</returns>
    string SuggestedName(string? existingIndexName = null);

    /// <summary>
    /// Generates a CREATE INDEX statement for the specified database dialect.
    /// </summary>
    /// <param name="dialect">The database dialect for which to generate the statement.</param>
    /// <returns>A CREATE INDEX statement as a string.</returns>
    string ToCreateStatement(PlanDialect dialect);
}