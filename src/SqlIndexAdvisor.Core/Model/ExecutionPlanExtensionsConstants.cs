namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Holds constant values used by <see cref="ExecutionPlanExtensions"/> to avoid magic values scattered throughout the code.
/// </summary>
internal static class ExecutionPlanExtensionsConstants
{
    /// <summary>
    /// The default <see cref="StringComparer"/> used for case‑insensitive string comparisons in execution‑plan analysis.
    /// </summary>
    public static readonly StringComparer OrdinalIgnoreCaseComparer = StringComparer.OrdinalIgnoreCase;
}
