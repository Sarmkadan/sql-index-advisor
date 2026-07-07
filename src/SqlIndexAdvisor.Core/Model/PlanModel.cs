namespace SqlIndexAdvisor.Core.Model;

/// <summary>
/// Database engine the plan came from. Rule thresholds differ slightly between the two.
/// </summary>
public enum PlanDialect
{
    SqlServer,
    Postgres
}

/// <summary>
/// A normalized execution plan. Both the SQL Server XML shape and the
/// Postgres JSON shape get flattened into this so the rules engine only has
/// to understand one thing.
/// </summary>
public sealed class ExecutionPlan
{
    public PlanDialect Dialect { get; init; }

    /// <summary>Raw statement text if the plan carried it. May be empty.</summary>
    public string StatementText { get; init; } = string.Empty;

    /// <summary>Optimizer estimated total cost for the statement.</summary>
    public double EstimatedTotalCost { get; init; }

    public List<PlanNode> Nodes { get; init; } = new();

    /// <summary>
    /// Missing-index hints emitted directly by the engine (SQL Server does this).
    /// These are a strong signal and get folded into the recommendations.
    /// </summary>
    public List<EngineMissingIndex> EngineMissingIndexes { get; init; } = new();
}

/// <summary>
/// One operator in the plan tree, flattened. We keep a parent pointer instead
/// of a child list because every rule we have walks bottom-up from scans.
/// </summary>
public sealed class PlanNode
{
    public string Operator { get; init; } = string.Empty;
    public string? TableName { get; init; }
    public string? IndexName { get; init; }

    /// <summary>Estimated rows the optimizer expects this node to emit.</summary>
    public double EstimatedRows { get; init; }

    /// <summary>Estimated rows read by this node (Postgres "Plan Rows" on the child scan).</summary>
    public double EstimatedRowsRead { get; init; }

    /// <summary>Fraction of the whole statement cost attributed to this node (0..1).</summary>
    public double RelativeCost { get; init; }

    /// <summary>Columns referenced in a residual predicate / filter, in order seen.</summary>
    public List<string> PredicateColumns { get; init; } = new();

    /// <summary>Columns the node ultimately outputs (candidates for INCLUDE).</summary>
    public List<string> OutputColumns { get; init; } = new();

    public PlanNode? Parent { get; set; }

    public bool IsScan =>
        Operator.Contains("Scan", StringComparison.OrdinalIgnoreCase) &&
        !Operator.Contains("Index Only", StringComparison.OrdinalIgnoreCase);
}

/// <summary>Missing index as reported by the engine's own optimizer.</summary>
public sealed class EngineMissingIndex
{
    public string Table { get; init; } = string.Empty;
    public double ImpactPercent { get; init; }
    public List<string> EqualityColumns { get; init; } = new();
    public List<string> InequalityColumns { get; init; } = new();
    public List<string> IncludeColumns { get; init; } = new();
}
