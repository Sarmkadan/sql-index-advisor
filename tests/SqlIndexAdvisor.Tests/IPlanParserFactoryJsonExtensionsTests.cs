using System;

namespace SqlIndexAdvisor.Tests
{
    public interface IPlanParserFactoryJsonExtensionsTests
    {
        void ToJson_NullFactory_ThrowsArgumentNullException();
        void ToJson_Default_IsNonIndentedJson();
        void ToJson_IndentedTrue_ContainsNewlines();
        void FromJson_ValidJson_ReturnsFactory();
        void FromJson_NullOrEmpty_ThrowsArgumentException(string json);
        void TryFromJson_ValidJson_ReturnsTrueAndFactory();
        void TryFromJson_InvalidJson_ReturnsFalseAndNull();
        void TryFromJson_NullOrEmpty_ThrowsArgumentException(string json);
    }
}
