namespace Innovayse.Hostpanel.Application.Provisioning.Queries.GetControlPanelSsoUrl;

/// <summary>Generates a single-sign-on URL for accessing the control panel.</summary>
/// <param name="ServiceId">The client service identifier. Overlaid from the route.</param>
public sealed record GetControlPanelSsoUrlQuery(int ServiceId);
