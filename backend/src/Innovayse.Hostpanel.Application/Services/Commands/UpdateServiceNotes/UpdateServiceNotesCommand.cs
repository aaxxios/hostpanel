namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceNotes;

/// <summary>Staff replace the internal notes kept on a service, as saved from the notes section of the service page.</summary>
/// <param name="ServiceId">The service primary key; the route id wins over any id in the body.</param>
/// <param name="AdminNotes">The notes, or <see langword="null"/> to clear them.</param>
public sealed record UpdateServiceNotesCommand(int ServiceId, string? AdminNotes);
