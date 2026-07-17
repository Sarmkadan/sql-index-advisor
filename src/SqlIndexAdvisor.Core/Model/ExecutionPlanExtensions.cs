namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Extension methods for <see cref="ExecutionPlan"/> that provide common analysis operations
/// for identifying indexing opportunities and plan characteristics.
/// </summary>
public static class ExecutionPlanExtensions
{
    /// <summary>
    /// Gets all scan operations in the execution plan that are good candidates for indexing.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>An enumerable of scan nodes that are not index-only scans.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<PlanNode> GetScanCandidates(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Nodes.Where(node => node.IsScan);
    }

    /// <summary>
    /// Calculates the total estimated cost of all scan operations in the plan.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>The sum of estimated costs for all scan operations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static double GetTotalScanCost(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Nodes
            .Where(node => node.IsScan)
            .Sum(node => node.RelativeCost);
    }

    /// <summary>
    /// Gets all tables that are being scanned in the execution plan.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>An enumerable of table names that are being scanned.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<string> GetScannedTables(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Nodes
            .Where(node => node.IsScan && !string.IsNullOrEmpty(node.TableName))
            .Select(node => node.TableName!)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all columns referenced in predicates across all scan operations.
    /// These are prime candidates for index key columns.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>An enumerable of column names used in predicates.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<string> GetPredicateColumns(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Nodes
            .Where(node => node.IsScan)
            .SelectMany(node => node.PredicateColumns)
            .Where(col => !string.IsNullOrEmpty(col))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all columns that are output by scan operations but not used in predicates.
    /// These are good candidates for INCLUDE columns in covering indexes.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>An enumerable of column names that could be INCLUDE columns.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<string> GetIncludeCandidateColumns(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var predicateColumns = plan.GetPredicateColumns().ToHashSet(StringComparer.OrdinalIgnoreCase);

        return plan.Nodes
            .Where(node => node.IsScan)
            .SelectMany(node => node.OutputColumns)
            .Where(col => !string.IsNullOrEmpty(col) && !predicateColumns.Contains(col))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all equality columns from engine missing indexes as a flattened collection.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>An enumerable of column names from equality predicates.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<string> GetMissingIndexEqualityColumns(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.EngineMissingIndexes
            .SelectMany(idx => idx.EqualityColumns)
            .Where(col => !string.IsNullOrEmpty(col));
    }

    /// <summary>
    /// Gets all inequality columns from engine missing indexes as a flattened collection.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>An enumerable of column names from inequality predicates.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<string> GetMissingIndexInequalityColumns(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.EngineMissingIndexes
            .SelectMany(idx => idx.InequalityColumns)
            .Where(col => !string.IsNullOrEmpty(col));
    }

    /// <summary>
    /// Gets all INCLUDE columns from engine missing indexes as a flattened collection.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>An enumerable of column names for INCLUDE columns.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<string> GetMissingIndexIncludeColumns(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.EngineMissingIndexes
            .SelectMany(idx => idx.IncludeColumns)
            .Where(col => !string.IsNullOrEmpty(col));
    }

    /// <summary>
    /// Determines whether the execution plan has any scan operations that could benefit from indexes.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>True if there are scan operations that are not index-only scans; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static bool HasIndexableScans(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Nodes.Any(node => node.IsScan);
    }

    /// <summary>
    /// Gets the highest cost scan operation in the plan.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>The scan node with the highest relative cost, or null if no scans exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static PlanNode? GetHighestCostScan(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Nodes
            .Where(node => node.IsScan)
            .MaxBy(node => node.RelativeCost);
    }

    /// <summary>
    /// Gets all columns that appear in both predicates and output columns across scan operations.
    /// These columns are already being covered by the scan and don't need INCLUDE columns.
    /// </summary>
    /// <param name="plan">The execution plan to analyze.</param>
    /// <returns>An enumerable of column names that are covered by existing operations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<string> GetAlreadyCoveredColumns(this ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var predicateColumns = plan.GetPredicateColumns().ToHashSet(StringComparer.OrdinalIgnoreCase);

        return plan.Nodes
            .Where(node => node.IsScan)
            .SelectMany(node => node.OutputColumns.Where(col => predicateColumns.Contains(col)))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}