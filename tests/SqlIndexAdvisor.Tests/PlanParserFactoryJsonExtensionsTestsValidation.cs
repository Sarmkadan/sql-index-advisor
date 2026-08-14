using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides validation helpers for <see cref="PlanParserFactoryJsonExtensionsTests"/>.
/// </summary>
public static class PlanParserFactoryJsonExtensionsTestsValidation
{
    /// <summary>
    /// Validates the state of the supplied <see cref="PlanParserFactoryJsonExtensionsTests"/> instance.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of human‑readable problem descriptions.
    /// The list is empty when the instance is considered valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this PlanParserFactoryJsonExtensionsTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // The test class only contains methods; there are no mutable state members to validate.
        // Therefore, it is always considered valid.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the supplied <see cref="PlanParserFactoryJsonExtensionsTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this PlanParserFactoryJsonExtensionsTests value) =>
        value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the supplied <see cref="PlanParserFactoryJsonExtensionsTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are found. The exception message contains a
    /// semicolon‑separated list of the problems.
    /// </exception>
    public static void EnsureValid(this PlanParserFactoryJsonExtensionsTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", problems), nameof(value));
        }
    }
}
