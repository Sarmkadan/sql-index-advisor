using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides validation helpers for <see cref="RecommendationMergerTests"/>.
/// </summary>
public static class RecommendationMergerTestsValidation
{
    /// <summary>
    /// Validates the state of the <see cref="RecommendationMergerTests"/> instance and returns a list of human‑readable problems.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problem messages. Empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this RecommendationMergerTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // The test class only contains methods; there are no instance fields or properties to validate.
        // Therefore, it is always considered valid.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the <see cref="RecommendationMergerTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this RecommendationMergerTests value) =>
        value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the <see cref="RecommendationMergerTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance contains validation problems; the exception message lists those problems.</exception>
    public static void EnsureValid(this RecommendationMergerTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RecommendationMergerTests is invalid: {string.Join("; ", problems)}",
                nameof(value));
        }
    }
}
