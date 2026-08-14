using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlIndexAdvisor.Tests
{
    public static class RecommendationEngineJsonExtensionsTestsValidation
    {
        public static IReadOnlyList<string> Validate(this RecommendationEngineJsonExtensionsTests value)
        {
            var problems = new List<string>();

            ArgumentNullException.ThrowIfNull(value);

            if (value.ToJson_WithValidEngine_ReturnsNonEmptyJson == null)
            {
                problems.Add("ToJson_WithValidEngine_ReturnsNonEmptyJson returned null");
            }

            if (value.ToJson_WithIndentation_ProducesIndentedJson == null)
            {
                problems.Add("ToJson_WithIndentation_ProducesIndentedJson returned null");
            }

            if (value.ToJson_NullEngine_ThrowsArgumentNullException == null)
            {
                problems.Add("ToJson_NullEngine_ThrowsArgumentNullException returned null");
            }

            if (value.FromJson_ValidJson_ReturnsEngineInstance == null)
            {
                problems.Add("FromJson_ValidJson_ReturnsEngineInstance returned null");
            }

            if (value.FromJson_EmptyOrWhiteSpace_ReturnsNull == null)
            {
                problems.Add("FromJson_EmptyOrWhiteSpace_ReturnsNull returned null");
            }

            if (value.FromJson_NullJson_ThrowsArgumentNullException == null)
            {
                problems.Add("FromJson_NullJson_ThrowsArgumentNullException returned null");
            }

            if (value.TryFromJson_ValidJson_ReturnsTrueAndEngine == null)
            {
                problems.Add("TryFromJson_ValidJson_ReturnsTrueAndEngine returned null");
            }

            if (value.TryFromJson_InvalidJson_ReturnsFalseAndNull == null)
            {
                problems.Add("TryFromJson_InvalidJson_ReturnsFalseAndNull returned null");
            }

            if (value.TryFromJson_NullJson_ThrowsArgumentNullException == null)
            {
                problems.Add("TryFromJson_NullJson_ThrowsArgumentNullException returned null");
            }

            return problems;
        }

        public static bool IsValid(this RecommendationEngineJsonExtensionsTests value)
        {
            return Validate(value).Count == 0;
        }

        public static void EnsureValid(this RecommendationEngineJsonExtensionsTests value)
        {
            var problems = Validate(value);

            if (problems.Any())
            {
                throw new ArgumentException($"Invalid test: {string.Join(", ", problems)}");
            }
        }
    }
}
