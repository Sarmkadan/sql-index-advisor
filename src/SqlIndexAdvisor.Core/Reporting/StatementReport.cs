using System.Collections.Generic;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Reporting;

/// <summary>
/// The analysis result of one statement of a plan input: its position among the statements,
/// its execution plan and the index recommendations the engine produced for that plan.
/// </summary>
/// <param name="Index">The one-based position of the statement in the input.</param>
/// <param name="Plan">The execution plan of the statement.</param>
/// <param name="Recommendations">The index recommendations for <paramref name="Plan"/>.</param>
public sealed record StatementReport(int Index, ExecutionPlan Plan, IReadOnlyList<IndexRecommendation> Recommendations);
