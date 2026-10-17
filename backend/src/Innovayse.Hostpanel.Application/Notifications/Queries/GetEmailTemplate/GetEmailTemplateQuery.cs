namespace Innovayse.Hostpanel.Application.Notifications.Queries.GetEmailTemplate;

/// <summary>Query to retrieve a single email template by its identifier.</summary>
/// <param name="Id">The identifier of the template to retrieve.</param>
public sealed record GetEmailTemplateQuery(int Id);
