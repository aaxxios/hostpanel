namespace Innovayse.Hostpanel.Application.Support.Commands.CreatePredefinedReplyCategory;

/// <summary>Command to create a new predefined reply category.</summary>
/// <param name="Name">The category name.</param>
/// <param name="ParentCategoryId">FK to the parent category, or <see langword="null"/> for a root category.</param>
public sealed record CreatePredefinedReplyCategoryCommand(string Name, int? ParentCategoryId);
