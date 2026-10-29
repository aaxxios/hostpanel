namespace Innovayse.Hostpanel.Application.Provisioning.Common;

/// <summary>A time-limited control-panel single-sign-on address.</summary>
/// <param name="Url">The address that signs the holder into the hosting control panel.</param>
public sealed record ControlPanelSsoDto(string Url);
