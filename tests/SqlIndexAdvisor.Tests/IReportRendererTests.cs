using SqlIndexAdvisor.Core.Model;
using SqlIndexAdvisor.Core.Reporting;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    public interface IReportRendererTests
    {
        void RenderText_EmptyRecommendations_ReturnsNoRecommendationsMessage();
        void RenderText_SingleRecommendation_IncludesTableName();
        void RenderText_SingleRecommendation_IncludesColumns();
        void RenderText_MultipleRecommendations_AllPresent();
        void RenderText_IncludeColumnsDistinctFromKeyColumns();
        void RenderText_IncludesPlanMetadata();
        void RenderJson_EmptyRecommendations_IncludesAllFields();
        void RenderJson_SingleRecommendation_IncludesAllRequiredFields();
        void RenderJson_MultipleRecommendations_AllPresent();
        void RenderJson_OutputIsValidJson();
        void RenderJson_IncludeColumnsDistinctFromKeyColumns();
        void RenderText_IncludesImpactDisclaimer();
    }
}
