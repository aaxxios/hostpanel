namespace Innovayse.Hostpanel.Domain.Projects;

/// <summary>
/// Refusal codes the <see cref="Project"/> aggregate throws as
/// <see cref="Innovayse.Hostpanel.Domain.DomainRuleException"/>.
/// </summary>
/// <remarks>
/// Declared here, not in <c>Application.Common.Errors.HostpanelErrorCodes</c>, because Domain may
/// not reference Application; <c>HostpanelErrorCodes</c> re-declares the same string values from
/// these constants so the resource coverage test still requires a sentence for each one.
/// </remarks>
public static class ProjectErrors
{
    /// <summary>The sort order is negative (400): <see cref="Project.Create"/>, <see cref="Project.Update"/> and <see cref="Project.SetSortOrder"/>.</summary>
    public const string SortOrderNegative = "PROJECT_SORT_ORDER_NEGATIVE";

    /// <summary>The slug is empty or whitespace (400): <see cref="Project.Create"/> and <see cref="Project.Update"/>.</summary>
    public const string SlugRequired = "PROJECT_SLUG_REQUIRED";
}
