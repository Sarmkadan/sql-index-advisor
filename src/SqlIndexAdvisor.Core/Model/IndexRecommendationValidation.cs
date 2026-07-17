using System.Globalization;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Provides validation helpers for <see cref="IndexRecommendation"/> instances.
/// </summary>
public static class IndexRecommendationValidation
{
    /// <summary>
    /// Validates an <see cref="IndexRecommendation"/> instance and returns a list of human-readable validation problems.
    /// Returns an empty list if the instance is valid.
    /// </summary>
    /// <param name="value">The recommendation to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this IndexRecommendation value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Table
        if (string.IsNullOrWhiteSpace(value.Table))
        {
            problems.Add("Table property must be a non-empty string.");
        }

        // Validate KeyColumns
        if (value.KeyColumns is null)
        {
            problems.Add("KeyColumns collection must not be null.");
        }
        else if (value.KeyColumns.Count == 0)
        {
            problems.Add("KeyColumns collection must contain at least one column.");
        }
        else
        {
            foreach (var column in value.KeyColumns)
            {
                if (string.IsNullOrWhiteSpace(column))
                {
                    problems.Add("All KeyColumns must be non-empty strings.");
                    break;
                }
            }
        }

        // Validate IncludeColumns (optional)
        if (value.IncludeColumns is not null)
        {
            foreach (var column in value.IncludeColumns)
            {
                if (string.IsNullOrWhiteSpace(column))
                {
                    problems.Add("All IncludeColumns must be non-empty strings.");
                    break;
                }
            }
        }

        // Validate EstimatedImpactPercent
        if (value.EstimatedImpactPercent < 0 || value.EstimatedImpactPercent > 100)
        {
            problems.Add("EstimatedImpactPercent must be between 0 and 100 inclusive.");
        }

        // Validate Confidence
        // Confidence is an enum, so it's always valid

        // Validate Reasons (optional)
        if (value.Reasons is not null)
        {
            foreach (var reason in value.Reasons)
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    problems.Add("All Reasons must be non-empty strings.");
                    break;
                }
            }
        }

        // Validate SuggestedName (computed property, but validate its components were valid)
        // This is validated indirectly through the other properties

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="IndexRecommendation"/> instance is valid.
    /// </summary>
    /// <param name="value">The recommendation to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this IndexRecommendation value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="IndexRecommendation"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The recommendation to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, with a detailed message.</exception>
    public static void EnsureValid(this IndexRecommendation value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"IndexRecommendation is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}