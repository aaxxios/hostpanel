using Innovayse.Hostpanel.Application.Domains.Common.Enums;

namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>Result of a domain availability check returned to the client.</summary>
/// <param name="Domain">The fully-qualified domain name that was checked (e.g. "example.com").</param>
/// <param name="Available">Whether the domain is available for registration.</param>
/// <param name="Status">The same outcome as a closed value, <see cref="DomainAvailabilityStatus"/>.</param>
public record DomainCheckResultDto(string Domain, bool Available, DomainAvailabilityStatus Status);
