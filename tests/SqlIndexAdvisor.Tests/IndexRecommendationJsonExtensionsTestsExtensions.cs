using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides extension methods for <see cref="IndexRecommendationJsonExtensionsTests"/>.
/// </summary>
public static class IndexRecommendationJsonExtensionsTestsExtensions
{
    /// <summary>
    /// Performs a round-trip serialization and deserialization and asserts equality of the recommendation.
    /// </summary>
    /// <param name="testContext">The test class instance.</param>
    /// <param name="original">The original <see cref="IndexRecommendation"/> to test.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="original"/> is null.</exception>
    public static void AssertRoundTrip(this IndexRecommendationJsonExtensionsTests _, IndexRecommendation original)
    {
        ArgumentNullException.ThrowIfNull(original);

        var json = original.ToJson();
        var deserialized = IndexRecommendationJsonExtensions.FromJson(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Table, deserialized.Table);
        Assert.Equal(original.KeyColumns, deserialized.KeyColumns);
        Assert.Equal(original.IncludeColumns, deserialized.IncludeColumns);
        Assert.Equal(original.EstimatedImpactPercent, deserialized.EstimatedImpactPercent);
        Assert.Equal(original.SourceNodeCost, deserialized.SourceNodeCost);
        Assert.Equal(original.Confidence, deserialized.Confidence);
        Assert.Equal(original.Reasons, deserialized.Reasons);
    }

    /// <summary>
    /// Asserts that the provided JSON string contains the specified key-value pair.
    /// </summary>
    /// <param name="testContext">The test class instance.</param>
    /// <param name="json">The JSON string to check.</param>
    /// <param name="key">The JSON key to find.</param>
    /// <param name="expectedValue">The expected value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> or <paramref name="key"/> is null.</exception>
    public static void AssertJsonContains(this IndexRecommendationJsonExtensionsTests _, string json, string key, string expectedValue)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(key);

        Assert.Contains($"\"{key}\":{expectedValue}", json);
    }
}
