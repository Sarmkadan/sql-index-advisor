using System;

namespace SqlIndexAdvisor.Tests
{
    internal static class PlanParserFactoryExtensionsTestsConstants
    {
        public const string ValidXml = "<ShowPlanXML xmlns=\"http://schemas.microsoft.com/sqlserver/2004/07/showplan\"><Batch></Batch></ShowPlanXML>";
        public const string ValidJson = "{\"Plan\":{}}";
        public const string InvalidContent = "not a plan";
    }
}
