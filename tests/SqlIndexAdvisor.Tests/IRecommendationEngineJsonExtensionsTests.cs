using System;
using SqlIndexAdvisor.Core.Engine;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    public interface IRecommendationEngineJsonExtensionsTests
    {
        void ToJson_WithValidEngine_ReturnsNonEmptyJson();
        void ToJson_WithIndentation_ProducesIndentedJson();
        void ToJson_NullEngine_ThrowsArgumentNullException();
        void FromJson_ValidJson_ReturnsEngineInstance();
        void FromJson_EmptyOrWhiteSpace_ReturnsNull();
        void FromJson_NullJson_ThrowsArgumentNullException();
        void TryFromJson_ValidJson_ReturnsTrueAndEngine();
        void TryFromJson_InvalidJson_ReturnsFalseAndNull();
        void TryFromJson_NullJson_ThrowsArgumentNullException();
    }
}
