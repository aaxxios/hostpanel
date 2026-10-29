namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>
/// The typed data an email template renders against. One sealed record per template derives from it.
/// </summary>
/// <remarks>
/// Members are PascalCase in C# and reach Liquid in lower snake case (<c>ResetLink</c> is
/// <c>{{ reset_link }}</c>), the one naming convention every stored template uses. The names
/// <c>brand</c> and <c>site</c> are reserved for the renderer, so no model declares them.
/// </remarks>
public abstract record EmailTemplateModel;
