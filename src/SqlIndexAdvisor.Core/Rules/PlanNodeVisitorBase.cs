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
            var result = Visit(node);
            if (result != null)
            {
                recommendations.AddRange(result);
            }
        }

        return recommendations;
    }

    /// <summary>
    /// Called for each node in the plan tree.
    /// Returns recommendations for nodes this visitor cares about, or null to skip.
    /// </summary>
    protected virtual IEnumerable<IndexRecommendation>? Visit(PlanNode node)
    {
        if (ShouldVisit(node))
        {
            return VisitCore(node);
        }
        return null;
    }

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