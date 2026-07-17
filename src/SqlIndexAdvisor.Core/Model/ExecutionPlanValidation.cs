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
            problems.Add($"Invalid Dialect value: {value.Dialect}. Expected SqlServer or Postgres.");
        }

        // Validate StatementText
        if (value.StatementText is null)
        {
            problems.Add("StatementText cannot be null.");
        }

        // Validate EstimatedTotalCost
        if (double.IsNaN(value.EstimatedTotalCost))
        {
            problems.Add("EstimatedTotalCost cannot be NaN.");
        }
        else if (double.IsInfinity(value.EstimatedTotalCost))
        {
            problems.Add("EstimatedTotalCost cannot be infinite.");
        }
        else if (value.EstimatedTotalCost < 0)
        {
            problems.Add("EstimatedTotalCost cannot be negative.");
        }

        // Validate Nodes collection
        if (value.Nodes is null)
        {
            problems.Add("Nodes collection cannot be null.");
        }
        else
        {
            // Validate each node
            for (var i = 0; i < value.Nodes.Count; i++)
            {
                var node = value.Nodes[i];
                if (node is null)
                {
                    problems.Add($"Nodes[{i}] cannot be null.");
                    continue;
                }

                if (string.IsNullOrEmpty(node.Operator))
                {
                    problems.Add($"Nodes[{i}].Operator cannot be null or empty.");
                }

                if (node.EstimatedRows < 0)
                {
                    problems.Add($"Nodes[{i}].EstimatedRows cannot be negative. Actual: {node.EstimatedRows}");
                }

                if (double.IsNaN(node.EstimatedRows))
                {
                    problems.Add($"Nodes[{i}].EstimatedRows cannot be NaN.");
                }

                if (double.IsInfinity(node.EstimatedRows))
                {
                    problems.Add($"Nodes[{i}].EstimatedRows cannot be infinite.");
                }

                if (node.EstimatedRowsRead < 0)
                {
                    problems.Add($"Nodes[{i}].EstimatedRowsRead cannot be negative. Actual: {node.EstimatedRowsRead}");
                }

                if (double.IsNaN(node.EstimatedRowsRead))
                {
                    problems.Add($"Nodes[{i}].EstimatedRowsRead cannot be NaN.");
                }

                if (double.IsInfinity(node.EstimatedRowsRead))
                {
                    problems.Add($"Nodes[{i}].EstimatedRowsRead cannot be infinite.");
                }

                if (node.RelativeCost < 0 || node.RelativeCost > 1)
                {
                    problems.Add($"Nodes[{i}].RelativeCost must be between 0 and 1. Actual: {node.RelativeCost}");
                }

                if (double.IsNaN(node.RelativeCost))
                {
                    problems.Add($"Nodes[{i}].RelativeCost cannot be NaN.");
                }

                if (double.IsInfinity(node.RelativeCost))
                {
                    problems.Add($"Nodes[{i}].RelativeCost cannot be infinite.");
                }

                // Validate string collections
                ValidateStringCollection(node.PredicateColumns, $"Nodes[{i}].PredicateColumns", problems, i);
                ValidateStringCollection(node.OutputColumns, $"Nodes[{i}].OutputColumns", problems, i);
            }
        }

        // Validate EngineMissingIndexes collection
        if (value.EngineMissingIndexes is null)
        {
            problems.Add("EngineMissingIndexes collection cannot be null.");
        }
        else
        {
            // Validate each missing index
            for (var i = 0; i < value.EngineMissingIndexes.Count; i++)
            {
                var index = value.EngineMissingIndexes[i];
                if (index is null)
                {
                    problems.Add($"EngineMissingIndexes[{i}] cannot be null.");
                    continue;
                }

                if (string.IsNullOrEmpty(index.Table))
                {
                    problems.Add($"EngineMissingIndexes[{i}].Table cannot be null or empty.");
                }

                if (index.ImpactPercent < 0 || index.ImpactPercent > 100)
                {
                    problems.Add($"EngineMissingIndexes[{i}].ImpactPercent must be between 0 and 100. Actual: {index.ImpactPercent}");
                }

                if (double.IsNaN(index.ImpactPercent))
                {
                    problems.Add($"EngineMissingIndexes[{i}].ImpactPercent cannot be NaN.");
                }

                if (double.IsInfinity(index.ImpactPercent))
                {
                    problems.Add($"EngineMissingIndexes[{i}].ImpactPercent cannot be infinite.");
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

    private static void ValidateStringCollection(IEnumerable<string> collection, string collectionPath, List<string> problems, int index)
    {
        if (collection is null)
        {
            problems.Add($"{collectionPath} cannot be null.");
            return;
        }

        var list = collection.ToList();
        for (var j = 0; j < list.Count; j++)
        {
            var item = list[j];
            if (string.IsNullOrEmpty(item))
            {
                problems.Add($"{collectionPath}[{j}] cannot be null or empty.");
            }
        }
    }
}