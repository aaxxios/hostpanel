namespace Innovayse.Hostpanel.Application.Faqs.Commands.DeleteFaq;

/// <summary>Command to permanently delete a question by id.</summary>
/// <param name="Id">The identifier of the question to delete.</param>
public sealed record DeleteFaqCommand(int Id);
