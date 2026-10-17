namespace Innovayse.Hostpanel.Application.Products.Commands.CreateProductGroup;

/// <summary>Command to create a new product group.</summary>
/// <param name="Name">Display name for the group.</param>
/// <param name="Description">Optional description.</param>
public sealed record CreateProductGroupCommand(string Name, string? Description);
