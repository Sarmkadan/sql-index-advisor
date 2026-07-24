using System;
using System.Collections.Generic;
using System.Globalization;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Provides extension methods for <see cref="ExecutionPlanValidationTests"/> to facilitate additional validation scenarios
/// and fluent assertions for execution plan validation testing.
/// </summary>
public static class ExecutionPlanValidationTestsExtensions
{
    /// <summary>
    /// Validates that all problems returned by the validation method contain the specified expected error message.
    /// </summary>
    /// <param name="problems">The collection of validation problems to check.</param>
    /// <param name="expectedMessage">The expected error message substring that should be present in all problems.</param>
    /// <returns>An enumerable of the actual problems for further assertions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="problems"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expectedMessage"/> is null.</exception>
    public static IEnumerable<string> ShouldAllContain(this IEnumerable<string> problems, string expectedMessage)
    {
        ArgumentNullException.ThrowIfNull(problems);
        ArgumentNullException.ThrowIfNull(expectedMessage);

        var problemList = new List<string>();
        foreach (var problem in problems)
        {
            if (problem is null)
            {
                throw new ArgumentException("Problem collection contains a null element", nameof(problems));
            }

            if (!problem.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Expected all problems to contain '{expectedMessage}', but found: '{problem}'");
            }

            problemList.Add(problem);
        }

        return problemList.AsReadOnly();
    }

    /// <summary>
    /// Validates that the execution plan has at least one node with the specified operator type.
    /// </summary>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="operatorName">The operator name to search for in the plan nodes.</param>
    /// <returns>True if at least one node with the specified operator exists; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operatorName"/> is null.</exception>
    public static bool HasOperator(this ExecutionPlan plan, string operatorName)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(operatorName);

        return plan.Nodes?.Count(node =>
            node?.Operator?.Equals(operatorName, StringComparison.OrdinalIgnoreCase) == true) > 0;
    }

    /// <summary>
    /// Validates that the execution plan has at least one node with the specified table name.
    /// </summary>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="tableName">The table name to search for in the plan nodes.</param>
    /// <returns>True if at least one node with the specified table name exists; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is null.</exception>
    public static bool HasTable(this ExecutionPlan plan, string tableName)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(tableName);

        return plan.Nodes?.Count(node =>
            node?.TableName?.Equals(tableName, StringComparison.OrdinalIgnoreCase) == true) > 0;
    }

    /// <summary>
    /// Validates that the execution plan has at least one node with the specified output column.
    /// </summary>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="columnName">The output column name to search for in the plan nodes.</param>
    /// <returns>True if at least one node with the specified output column exists; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="columnName"/> is null.</exception>
    public static bool HasOutputColumn(this ExecutionPlan plan, string columnName)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(columnName);

        return plan.Nodes?.Any(node =>
            node?.OutputColumns?.Contains(columnName, StringComparer.OrdinalIgnoreCase) == true) == true;
    }

    /// <summary>
    /// Validates that the execution plan has at least one node with the specified predicate column.
    /// </summary>
    /// <param name="plan">The execution plan to validate.</param>
    /// <param name="columnName">The predicate column name to search for in the plan nodes.</param>
    /// <returns>True if at least one node with the specified predicate column exists; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="columnName"/> is null.</exception>
    public static bool HasPredicateColumn(this ExecutionPlan plan, string columnName)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(columnName);

        return plan.Nodes?.Any(node =>
            node?.PredicateColumns?.Contains(columnName, StringComparer.OrdinalIgnoreCase) == true) == true;
    }
}