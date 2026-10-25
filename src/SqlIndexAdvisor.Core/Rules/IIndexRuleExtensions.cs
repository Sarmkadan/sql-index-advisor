namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Provides extension methods for <see cref="IIndexRule"/>.
/// </summary>
public static class IIndexRuleExtensions
{
    /// <summary>
    /// Gets a human-readable name for the rule, suitable for logs and diagnostics.
    /// Uses <see cref="IIndexRule.Name"/> when it is set; otherwise falls back to the implementing type's name.
    /// </summary>
    /// <param name="rule">The rule to describe.</param>
    /// <returns>The rule's name, or its type name when the name is null, empty, or white space.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rule"/> is null.</exception>
    public static string GetDisplayName(this IIndexRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return string.IsNullOrWhiteSpace(rule.Name)
            ? rule.GetType().Name
            : rule.Name;
    }
}
