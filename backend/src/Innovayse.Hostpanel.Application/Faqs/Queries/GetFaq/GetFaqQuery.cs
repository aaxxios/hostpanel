namespace Innovayse.Hostpanel.Application.Faqs.Queries.GetFaq;

/// <summary>Query to retrieve a single question by id for the admin panel.</summary>
/// <param name="Id">The identifier of the question to retrieve.</param>
public sealed record GetFaqQuery(int Id);
