namespace Innovayse.Hostpanel.Application.Domains.Commands.SetEmailForwarding;

/// <summary>Command to enable or disable email forwarding for a domain.</summary>
/// <param name="DomainId">
/// Primary key of the domain to update. Nullable so the route id can overlay it: a non-nullable
/// member here would make the body required before <c>with { DomainId = id }</c> ever ran.
/// </param>
/// <param name="Enabled">Whether email forwarding should be enabled.</param>
public record SetEmailForwardingCommand(int? DomainId, bool Enabled);
