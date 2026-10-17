namespace Innovayse.Hostpanel.Application.Support.Commands.CreateDownloadCategory;

/// <summary>Command to create a new download category.</summary>
/// <param name="Name">The category display name.</param>
/// <param name="Description">The category description text.</param>
/// <param name="IsHidden">Whether the category should be hidden from clients.</param>
/// <param name="ParentCategoryId">Optional parent category id for nesting.</param>
public sealed record CreateDownloadCategoryCommand(
    string Name, string Description, bool IsHidden, int? ParentCategoryId = null);
