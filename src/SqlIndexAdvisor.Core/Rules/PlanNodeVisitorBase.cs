using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Rules;

/// <summary>
/// Base class for plan node visitors that implement IIndexRule.
/// Provides common infrastructure for rules that visit plan nodes.
/// </summary>
public abstract class PlanNodeVisitorBase : IIndexRule
{
    public virtual string Name => GetType().Name.ToLowerInvariant().Replace("rule", "");

    public virtual IEnumerable<IndexRecommendation> Evaluate(ExecutionPlan plan)
    {
        var recommendations = new List<IndexRecommendation>();

        // Visit all nodes once using the visitor pattern
        foreach (var node in plan.Nodes)
        {
            if (ShouldVisit(node))
            {
                recommendations.AddRange(VisitCore(node, plan));
            }
        }

        return recommendations;
    }

    /// <summary>
    /// Plan-aware visit hook. Rules that need to look at siblings or descendants
    /// (anything beyond the parent chain) override this; the default delegates to
    /// the node-only overload.
    /// </summary>
    protected virtual IEnumerable<IndexRecommendation> VisitCore(PlanNode node, ExecutionPlan plan) =>
        VisitCore(node);

    /// <summary>
    /// Determines whether this visitor should process the given node.
    /// Override this to implement custom filtering logic.
    /// </summary>
    protected abstract bool ShouldVisit(PlanNode node);

    /// <summary>
    /// Called when a node passes ShouldVisit check.
    /// Override this to implement the actual visiting logic.
    /// </summary>
    protected abstract IEnumerable<IndexRecommendation> VisitCore(PlanNode node);

    /// <summary>
    /// Gets the chain of parent nodes from the current node up to the root.
    /// </summary>
    /// <summary>
    /// Gets all nodes in the plan that sit below the given node in the tree
    /// (i.e. whose parent chain passes through it).
    /// </summary>
    protected static IEnumerable<PlanNode> GetDescendants(PlanNode node, ExecutionPlan plan) =>
        plan.Nodes.Where(n => GetParentChain(n).Contains(node));

    protected static IEnumerable<PlanNode> GetParentChain(PlanNode node)
    {
        var current = node.Parent;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }
}