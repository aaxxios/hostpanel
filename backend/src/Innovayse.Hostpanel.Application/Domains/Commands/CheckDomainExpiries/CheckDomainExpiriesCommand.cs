using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Domains.Commands.CheckDomainExpiries;

/// <summary>Scheduled command that scans for expired domains and marks them accordingly.</summary>
public sealed record CheckDomainExpiriesCommand : IRecurringJob;
