namespace Innovayse.Hostpanel.Application.Services.Commands.CorrectServiceTerminationDate;

/// <summary>
/// Staff correct the recorded termination date of a terminated service, for a termination that really
/// happened on another day than the one it was recorded on.
/// </summary>
/// <param name="ServiceId">The service primary key; the route id wins over any id in the body.</param>
/// <param name="TerminatedAt">The moment the service really ended.</param>
public sealed record CorrectServiceTerminationDateCommand(int ServiceId, DateTimeOffset TerminatedAt);
