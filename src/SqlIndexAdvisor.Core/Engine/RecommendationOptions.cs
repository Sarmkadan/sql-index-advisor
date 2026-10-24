namespace SqlIndexAdvisor.Core.Engine;

/// <summary>
/// Thresholds that decide when a scan is worth an index recommendation. Scans below the table-size
/// floor, or whose filter keeps too many rows, are suppressed; scans near the selectivity limit are
/// kept with lower confidence. Thresholds only apply when the plan exposes row counts for the node.
/// </summary>
public sealed class RecommendationOptions
{
    /// <summary>The default for <see cref="MinTableRows"/>.</summary>
    public const double DefaultMinTableRows = 1_000;

    /// <summary>The default for <see cref="MaxSelectivity"/>.</summary>
    public const double DefaultMaxSelectivity = 0.5;

    private double _minTableRows = DefaultMinTableRows;
    private double _maxSelectivity = DefaultMaxSelectivity;

    /// <summary>
    /// Minimum number of rows a scan must read before an index is recommended for it. A scan of a smaller
    /// table is already cheap, so an index would not pay for itself. Zero disables the check.
    /// Defaults to <see cref="DefaultMinTableRows"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative or not finite.</exception>
    public double MinTableRows
    {
        get => _minTableRows;
        init
        {
            if (!double.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value), value, "MinTableRows must be a finite number.");
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _minTableRows = value;
        }
    }

    /// <summary>
    /// Largest fraction of the rows read that a filter may return and still earn a recommendation (0, 1].
    /// A filter returning more than this share of the rows gains little from an index. Scans returning more
    /// than half of this limit are kept with confidence lowered one level.
    /// Defaults to <see cref="DefaultMaxSelectivity"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is not in the range (0, 1].</exception>
    public double MaxSelectivity
    {
        get => _maxSelectivity;
        init
        {
            if (!double.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value), value, "MaxSelectivity must be a finite number.");
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0.0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 1.0);
            _maxSelectivity = value;
        }
    }
}
