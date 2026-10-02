using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Base class for plan node visitors that implement IIndexRule.
/// Provides common infrastructure for rules that visit plan nodes.
/// </summary>
public abstract class PlanNodeVisitorBase : IIndexRule
{
    /// <summary>
    /// Gets the name of the rule.
    /// </summary>
    public virtual string Name => GetType().Name.ToLowerInvariant().Replace("rule", "");

    /// <summary>
    /// Evaluates this rule against the given execution plan.
    /// </summary>
    public virtual IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var recommendations = new List<IndexRecommendation>();
        foreach (var node in plan.Nodes)
        {
            if (ShouldVisit(node))
            {
                foreach (var rec in VisitCore(node, plan))
                {
                    rec.Rule ??= Name;
                    recommendations.Add(rec);
                }
            }
        }
        return recommendations;
    }

    /// <summary>
    /// Plan-aware visit hook.
    /// </summary>
    protected virtual IEnumerable<IndexRecommendation> VisitCore(PlanNode node, ExecutionPlan plan) =>
        VisitCore(node);

    /// <summary>
    /// Determines whether this visitor should process the given node.
    /// </summary>
    protected abstract bool ShouldVisit(PlanNode node);

    /// <summary>
    /// Called when a node passes ShouldVisit check.
    /// </summary>
    protected abstract IEnumerable<IndexRecommendation> VisitCore(PlanNode node);

    /// <summary>
    /// Gets the chain of parent nodes from the current node up to the root.
    /// </summary>
    protected static IEnumerable<PlanNode> GetParentChain(PlanNode node)
    {
        var current = node.Parent;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    /// <summary>
    /// Gets all nodes in the plan that sit below the given node in the tree.
    /// </summary>
    protected static IEnumerable<PlanNode> GetDescendants(PlanNode node, ExecutionPlan plan) =>
        plan.Nodes.Where(n => GetParentChain(n).Contains(node));
}
