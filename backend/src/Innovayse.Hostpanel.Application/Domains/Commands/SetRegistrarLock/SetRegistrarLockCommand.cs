namespace Innovayse.Hostpanel.Application.Domains.Commands.SetRegistrarLock;

/// <summary>Command to enable or disable the registrar transfer-lock for a domain.</summary>
/// <param name="DomainId">
/// Primary key of the domain to update. Nullable so the route id can overlay it: a non-nullable
/// member here would make the body required before <c>with { DomainId = id }</c> ever ran.
/// </param>
/// <param name="Enabled">Whether the domain should be locked.</param>
public record SetRegistrarLockCommand(int? DomainId, bool Enabled);
