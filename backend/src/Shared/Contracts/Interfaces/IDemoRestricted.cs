namespace Innovayse.Hostpanel.Shared.Contracts.Interfaces;

/// <summary>
/// Marker for a command a public demo must refuse: settings and credentials, branding uploads,
/// staff accounts and the demo accounts' own sign-in details. The demo guard policy puts its
/// middleware on exactly the handler chains whose message carries this marker, so a handler never
/// tests for demo mode itself, and a new dangerous command is covered by one word on its record.
/// </summary>
public interface IDemoRestricted
{
}
