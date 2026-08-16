using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Provides validation helpers for <see cref="ExecutionPlan"/> instances.
/// Validates that execution plans contain meaningful data and that their numeric values
/// fall within expected ranges for SQL execution plans.
/// </summary>
public static class ExecutionPlanValidation
{
    /// <summary>
    /// Validates that the execution plan contains meaningful data and that all values
    /// fall within expected ranges. Returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The execution plan to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if the plan is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ExecutionPlan value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Dialect
        if (value.Dialect != PlanDialect.SqlServer && value.Dialect != PlanDialect.Postgres)
        {
            problems.Add(string.Format(ExecutionPlanValidationConstants.InvalidDialectMessage, value.Dialect));
        }

        // Validate StatementText
        if (value.StatementText is null)
        {
            problems.Add(ExecutionPlanValidationConstants.StatementTextCannotBeNull);
        }

        // Validate EstimatedTotalCost
        if (double.IsNaN(value.EstimatedTotalCost))
        {
            problems.Add(ExecutionPlanValidationConstants.EstimatedTotalCostNaN);
        }
        else if (double.IsInfinity(value.EstimatedTotalCost))
        {
            problems.Add(ExecutionPlanValidationConstants.EstimatedTotalCostInfinite);
        }
        else if (value.EstimatedTotalCost < 0)
        {
            problems.Add(ExecutionPlanValidationConstants.EstimatedTotalCostNegative);
        }

        // Validate Nodes collection
        if (value.Nodes is null)
        {
            problems.Add(ExecutionPlanValidationConstants.NodesCollectionCannotBeNull);
        }
        else
        {
            // Validate each node
            for (var i = 0; i < value.Nodes.Count; i++)
            {
                var node = value.Nodes[i];
                if (node is null)
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeCannotBeNull, i));
                    continue;
                }

                if (string.IsNullOrEmpty(node.Operator))
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeOperatorCannotBeNullOrEmpty, i));
                }

                if (node.EstimatedRows < 0)
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeEstimatedRowsNegative, i, node.EstimatedRows));
                }

                if (double.IsNaN(node.EstimatedRows))
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeEstimatedRowsNaN, i));
                }

                if (double.IsInfinity(node.EstimatedRows))
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeEstimatedRowsInfinite, i));
                }

                if (node.EstimatedRowsRead < 0)
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeEstimatedRowsReadNegative, i, node.EstimatedRowsRead));
                }

                if (double.IsNaN(node.EstimatedRowsRead))
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeEstimatedRowsReadNaN, i));
                }

                if (double.IsInfinity(node.EstimatedRowsRead))
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeEstimatedRowsReadInfinite, i));
                }

                if (node.RelativeCost < ExecutionPlanValidationConstants.RelativeCostMin ||
                    node.RelativeCost > ExecutionPlanValidationConstants.RelativeCostMax)
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeRelativeCostOutOfRange, i, node.RelativeCost));
                }

                if (double.IsNaN(node.RelativeCost))
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeRelativeCostNaN, i));
                }

                if (double.IsInfinity(node.RelativeCost))
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.NodeRelativeCostInfinite, i));
                }

                // Validate string collections
                ValidateStringCollection(node.PredicateColumns, $"Nodes[{i}].PredicateColumns", problems, i);
                ValidateStringCollection(node.OutputColumns, $"Nodes[{i}].OutputColumns", problems, i);
            }
        }

        // Validate EngineMissingIndexes collection
        if (value.EngineMissingIndexes is null)
        {
            problems.Add(ExecutionPlanValidationConstants.EngineMissingIndexesCollectionCannotBeNull);
        }
        else
        {
            // Validate each missing index
            for (var i = 0; i < value.EngineMissingIndexes.Count; i++)
            {
                var index = value.EngineMissingIndexes[i];
                if (index is null)
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.EngineMissingIndexCannotBeNull, i));
                    continue;
                }

                if (string.IsNullOrEmpty(index.Table))
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.EngineMissingIndexTableCannotBeNullOrEmpty, i));
                }

                if (index.ImpactPercent < ExecutionPlanValidationConstants.ImpactPercentMin ||
                    index.ImpactPercent > ExecutionPlanValidationConstants.ImpactPercentMax)
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.EngineMissingIndexImpactPercentOutOfRange, i, index.ImpactPercent));
                }

                if (double.IsNaN(index.ImpactPercent))
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.EngineMissingIndexImpactPercentNaN, i));
                }

                if (double.IsInfinity(index.ImpactPercent))
                {
                    problems.Add(string.Format(ExecutionPlanValidationConstants.EngineMissingIndexImpactPercentInfinite, i));
                }

                // Validate column collections
                ValidateStringCollection(index.EqualityColumns, $"EngineMissingIndexes[{i}].EqualityColumns", problems, i);
                ValidateStringCollection(index.InequalityColumns, $"EngineMissingIndexes[{i}].InequalityColumns", problems, i);
                ValidateStringCollection(index.IncludeColumns, $"EngineMissingIndexes[{i}].IncludeColumns", problems, i);
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the execution plan is valid.
    /// </summary>
    /// <param name="value">The execution plan to check.</param>
    /// <returns>True if the plan is valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ExecutionPlan value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the execution plan is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The execution plan to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the execution plan is invalid.</exception>
    public static void EnsureValid(this ExecutionPlan value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ExecutionPlan is invalid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }

    /// <summary>
    /// Validates that a collection of strings contains no null or empty entries.
    /// </summary>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="collectionPath">The path/identifier for error messages.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <param name="index">The node/missing index index for context in error messages.</param>
    private static void ValidateStringCollection(
        IEnumerable<string> collection,
        string collectionPath,
        List<string> problems,
        int index)
    {
        if (collection is null)
        {
            problems.Add(string.Format(ExecutionPlanValidationConstants.CollectionCannotBeNull, collectionPath));
            return;
        }

        var list = collection.ToList();
        for (var j = 0; j < list.Count; j++)
        {
            var item = list[j];
            if (string.IsNullOrEmpty(item))
            {
                problems.Add(string.Format(ExecutionPlanValidationConstants.CollectionItemCannotBeNullOrEmpty, collectionPath, j));
            }
        }
    }
}
