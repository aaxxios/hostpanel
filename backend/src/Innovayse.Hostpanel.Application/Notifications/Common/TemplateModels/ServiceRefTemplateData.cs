namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>A service an email template refers to by number.</summary>
/// <param name="Id">The service id, read as <c>id</c>.</param>
public sealed record ServiceRefTemplateData(int Id);
