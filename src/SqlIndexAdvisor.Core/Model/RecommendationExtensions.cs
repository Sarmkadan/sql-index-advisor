using System;
using SqlIndexAdvisor.Core.Model;

namespace SqlIndexAdvisor.Core.Model
{
    /// <summary>
    /// Extension methods for <see cref="IndexRecommendation"/>.
    /// </summary>
    public static class RecommendationExtensions
    {
        /// <summary>
        /// Returns a concise, one‑line summary of the recommendation.
        /// The implementation uses the suggested index name and the CREATE statement
        /// (defaulting to the SQL Server dialect). Adjust as needed for other dialects.
        /// </summary>
        public static string ToOneLineSummary(this IndexRecommendation recommendation)
        {
            if (recommendation == null) throw new ArgumentNullException(nameof(recommendation));

            // Use the suggested name and a CREATE statement as a simple one‑line summary.
            // The dialect can be changed later if required.
            var createStatement = recommendation.ToCreateStatement(PlanDialect.SqlServer);
            return $"{recommendation.SuggestedName()}: {createStatement}";
        }

        /// <summary>
        /// Returns a markdown table row representing the recommendation.
        /// Columns: severity badge, index name, CREATE statement (escaped for markdown).
        /// </summary>
        /// <param name="recommendation">The recommendation to render.</param>
        /// <param name="dialect">The SQL dialect to use for the CREATE statement. Defaults to SQL Server.</param>
        public static string ToMarkdownRow(this IndexRecommendation recommendation, PlanDialect dialect = PlanDialect.SqlServer)
        {
            if (recommendation == null) throw new ArgumentNullException(nameof(recommendation));

            var badge = recommendation.GetSeverityBadge();
            var name = recommendation.SuggestedName();
            var statement = recommendation.ToCreateStatement(dialect)
                                          .Replace("\n", " ")
                                          .Replace("\r", string.Empty)
                                          .Replace("|", "\\|"); // escape markdown pipe

            // Render as a markdown table row.
            return $"| {badge} | {name} | `{statement}` |";
        }

        /// <summary>
        /// Returns a markdown badge string representing the recommendation's severity.
        /// If the recommendation does not expose a <c>Severity</c> property, a generic "Info"
        /// badge is returned.
        /// </summary>
        public static string GetSeverityBadge(this IndexRecommendation recommendation)
        {
            if (recommendation == null) throw new ArgumentNullException(nameof(recommendation));

            // The concrete type may expose a Severity property (e.g., enum or string).
            // We use dynamic to avoid a hard compile‑time dependency.
            try
            {
                dynamic dyn = recommendation;
                // Attempt to read a property named "Severity". If it does not exist,
                // a RuntimeBinderException will be thrown and we fall back to the default.
                string? severity = null;

                // If the property is an enum, convert it to its name.
                var raw = dyn.Severity;
                if (raw != null)
                {
                    severity = raw is string s ? s : raw.ToString();
                }

                return BadgeFor(severity);
            }
            catch
            {
                // No Severity property – use the default badge.
                return BadgeFor(null);
            }
        }

        private static string BadgeFor(string? severity)
        {
            return severity?.ToLowerInvariant() switch
            {
                "critical" => "🛑 Critical",
                "high"     => "🔴 High",
                "medium"   => "🟠 Medium",
                "low"      => "🟢 Low",
                _          => "ℹ️ Info"
            };
        }
    }
}
