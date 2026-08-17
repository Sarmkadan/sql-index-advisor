using System.Text.Json;

namespace SqlIndexAdvisor.Tests
{
    internal static class RecommendationEngineTestsJsonExtensionsConstants
    {
        // JSON naming policy used for serialization (camelCase)
        public static readonly JsonNamingPolicy DefaultJsonNamingPolicy = JsonNamingPolicy.CamelCase;

        // String representation of the naming policy (kept for backward compatibility)
        public const string DefaultJsonNamingPolicyName = "camelCase";

        // Default indentation setting for JSON output
        public const bool DefaultWriteIndented = false;
    }
}
