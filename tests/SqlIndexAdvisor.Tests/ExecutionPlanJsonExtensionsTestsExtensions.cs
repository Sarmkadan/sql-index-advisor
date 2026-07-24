using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Extension methods for <see cref="ExecutionPlan"/> that provide additional functionality
/// for testing JSON serialization and deserialization scenarios.
/// </summary>
public static class ExecutionPlanJsonExtensionsTestsExtensions
{
    /// <summary>
    /// Creates a JSON string from the execution plan with culture-invariant formatting for numeric values.
    /// </summary>
    /// <param name="plan">The execution plan to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <param name="useInvariantCulture">Whether to use invariant culture for numeric formatting.</param>
    /// <returns>A JSON string representation of the execution plan.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static string ToJsonWithCulture(
        this ExecutionPlan plan,
        bool indented = false,
        bool useInvariantCulture = true)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var json = ExecutionPlanJsonExtensions.ToJson(plan, indented);

        if (useInvariantCulture)
        {
            json = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<JsonElement>(json),
                new JsonSerializerOptions { WriteIndented = indented });
        }

        return json;
    }

    /// <summary>
    /// Attempts to deserialize JSON and returns detailed information about the operation result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="result">The deserialized execution plan, or null if deserialization failed.</param>
    /// <param name="errorMessage">The error message if deserialization failed, otherwise null.</param>
    /// <returns>True if deserialization succeeded, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJsonWithDetails(
        string json,
        out ExecutionPlan? result,
        out string? errorMessage)
    {
        ArgumentNullException.ThrowIfNull(json);

        result = null;
        errorMessage = null;

        try
        {
            result = ExecutionPlanJsonExtensions.FromJson(json);
            return true;
        }
        catch (JsonException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
        catch (ArgumentException ex) when (ex.ParamName == "json")
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Validates that the execution plan's JSON representation contains the specified property.
    /// </summary>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="propertyName">The property name to check for (case-insensitive).</param>
    /// <returns>True if the property exists in the JSON, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null or empty.</exception>
    public static bool JsonContainsProperty(
        this ExecutionPlan plan,
        string propertyName)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        var json = ExecutionPlanJsonExtensions.ToJson(plan);
        return json.Contains(propertyName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the JSON size in bytes for the execution plan's JSON representation.
    /// </summary>
    /// <param name="plan">The execution plan to measure.</param>
    /// <param name="indented">Whether to use indented JSON formatting.</param>
    /// <returns>The size of the JSON representation in bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static int GetJsonSizeInBytes(
        this ExecutionPlan plan,
        bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var json = ExecutionPlanJsonExtensions.ToJson(plan, indented);
        return json.Length;
    }

    /// <summary>
    /// Creates a deep clone of the execution plan by serializing and deserializing it.
    /// </summary>
    /// <param name="plan">The execution plan to clone.</param>
    /// <returns>A deep clone of the execution plan.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static ExecutionPlan DeepClone(
        this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var json = ExecutionPlanJsonExtensions.ToJson(plan);
        return ExecutionPlanJsonExtensions.FromJson(json);
    }
}