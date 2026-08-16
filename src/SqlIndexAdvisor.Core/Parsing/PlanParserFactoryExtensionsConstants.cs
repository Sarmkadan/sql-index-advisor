namespace SqlIndexAdvisor.Core.Parsing;

/// <summary>
/// Contains constant values used by <see cref="PlanParserFactoryExtensions"/>.
/// </summary>
internal static class PlanParserFactoryExtensionsConstants
{
    /// <summary>
    /// The error message displayed when no parser is selected by the custom selector function.
    /// </summary>
    public const string NoParserSelectedErrorMessage = "No parser was selected by the provided selector function.";
}
