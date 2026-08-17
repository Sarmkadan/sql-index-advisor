using System.Text.Json;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Holds reusable constant values for <see cref="PredicateColumnScannerTestsJsonExtensions"/>.
    /// </summary>
    internal static class PredicateColumnScannerTestsJsonExtensionsConstants
    {
        /// <summary>
        /// Default <see cref="JsonSerializerOptions"/> used throughout the tests.
        /// </summary>
        public static readonly JsonSerializerOptions DefaultJsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// <see cref="JsonSerializerOptions"/> with indentation enabled.
        /// </summary>
        public static readonly JsonSerializerOptions IndentedJsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
    }
}
