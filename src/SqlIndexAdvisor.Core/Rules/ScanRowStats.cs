using System.Globalization;
using SqlIndexAdvisor.Core.Engine;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Row counts for one scan node, resolved once so the rules gate on the same numbers they report.
/// Runtime (actual) counts are preferred over optimizer estimates. <see cref="RowsRead"/> is <c>null</c>
/// when the plan carries no usable figure, and then no row threshold applies.
/// </summary>
/// <param name="RowsRead">Rows the scan reads, or <c>null</c> when unknown.</param>
/// <param name="RowsReturned">Rows the scan returns after its filter.</param>
/// <param name="IsActual">Whether the counts come from a runtime execution rather than the estimate.</param>
internal sealed record ScanRowStats(double? RowsRead, double RowsReturned, bool IsActual)
{
    /// <summary>
    /// Gets the fraction of the rows read that the filter returns, clamped to 0..1, or <c>null</c> when
    /// <see cref="RowsRead"/> is unknown.
    /// </summary>
    public double? MatchFraction => RowsRead is { } read
        ? Math.Clamp(RowsReturned / read, 0.0, 1.0)
        : null;

    /// <summary>
    /// Resolves the row counts for a scan node, preferring runtime counts when the plan has them.
    /// </summary>
    /// <param name="node">The scan node to read.</param>
    /// <returns>The resolved row counts for the node.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
    public static ScanRowStats From(PlanNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.ActualRowsRead > 0)
            return new(node.ActualRowsRead, node.ActualRows, IsActual: true);

        var estimatedRead = node.TableCardinality > 0
            ? node.TableCardinality
            : node.EstimatedRowsRead > 0 ? node.EstimatedRowsRead : (double?)null;
        return new(estimatedRead, node.EstimatedRows, IsActual: false);
    }

    /// <summary>
    /// Determines whether the scan reads fewer rows than the table-size floor, so an index would not pay off.
    /// </summary>
    /// <param name="options">The thresholds to compare against.</param>
    /// <returns><see langword="true"/> when the rows read are known and below <see cref="RecommendationOptions.MinTableRows"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public bool IsBelowMinTable(RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return RowsRead is { } read && read < options.MinTableRows;
    }

    /// <summary>
    /// Determines whether the filter keeps more rows than <see cref="RecommendationOptions.MaxSelectivity"/> allows.
    /// </summary>
    /// <param name="options">The thresholds to compare against.</param>
    /// <returns><see langword="true"/> when the match fraction is known and above the limit.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public bool IsUnselective(RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return MatchFraction is { } match && match > options.MaxSelectivity;
    }

    /// <summary>
    /// Determines whether the scan clears both suppression thresholds and may produce a recommendation.
    /// </summary>
    /// <param name="options">The thresholds to compare against.</param>
    /// <returns><see langword="true"/> when the scan is neither below the table-size floor nor unselective.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public bool IsWorthIndexing(RecommendationOptions options) =>
        !IsBelowMinTable(options) && !IsUnselective(options);

    /// <summary>
    /// Determines whether the filter keeps more than half of the selectivity limit, which marks the scan as
    /// marginal and lowers its confidence one level.
    /// </summary>
    /// <param name="options">The thresholds to compare against.</param>
    /// <returns><see langword="true"/> when the match fraction is known and above half of <see cref="RecommendationOptions.MaxSelectivity"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public bool IsMarginal(RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return MatchFraction is { } match && match > options.MaxSelectivity / 2;
    }

    /// <summary>
    /// Applies the marginal-selectivity downgrade to a confidence level.
    /// </summary>
    /// <param name="baseline">The confidence derived from the node's cost share.</param>
    /// <param name="options">The thresholds to compare against.</param>
    /// <returns><paramref name="baseline"/> lowered one level when the scan is marginal; otherwise <paramref name="baseline"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public Confidence AdjustConfidence(Confidence baseline, RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!IsMarginal(options))
            return baseline;
        return baseline switch
        {
            Confidence.High => Confidence.Medium,
            _ => Confidence.Low
        };
    }

    /// <summary>
    /// Describes the row evidence as human-readable reasons, so the numbers behind a suppression or a
    /// downgrade are visible in the output.
    /// </summary>
    /// <param name="options">The thresholds in effect, quoted in the downgrade reason.</param>
    /// <returns>One or two reason sentences.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public IReadOnlyList<string> Describe(RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (RowsRead is not { } read)
            return [ScanRowStatsConstants.UnknownRowCountsReason];

        var source = IsActual ? ScanRowStatsConstants.ActualSource : ScanRowStatsConstants.EstimatedSource;
        var matchPercent = FormatPercent(MatchFraction!.Value);
        var reasons = new List<string>
        {
            $"Scan reads {FormatRows(read)} rows ({source}); the filter returns {FormatRows(RowsReturned)} of them ({matchPercent}%)."
        };
        if (IsMarginal(options))
        {
            reasons.Add(
                $"Confidence lowered one level: the filter keeps {matchPercent}%, above half of the " +
                $"{FormatPercent(options.MaxSelectivity)}% selectivity limit.");
        }
        return reasons;
    }

    private static string FormatRows(double rows) =>
        rows.ToString(ScanRowStatsConstants.RowCountFormat, CultureInfo.InvariantCulture);

    private static string FormatPercent(double fraction) =>
        (fraction * 100.0).ToString(ScanRowStatsConstants.PercentFormat, CultureInfo.InvariantCulture);
}

/// <summary>
/// Text and number formats used when describing scan row counts.
/// </summary>
internal static class ScanRowStatsConstants
{
    public const string ActualSource = "actual";
    public const string EstimatedSource = "estimated";
    public const string RowCountFormat = "N0";
    public const string PercentFormat = "0.#";
    public const string UnknownRowCountsReason =
        "Row counts are not in the plan, so the table-size and selectivity thresholds were not applied.";
}
