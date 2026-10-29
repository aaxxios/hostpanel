namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>A domain an email template refers to by name.</summary>
/// <param name="Name">The domain name, read as <c>name</c>.</param>
public sealed record DomainNameTemplateData(string Name);
