namespace Innovayse.Hostpanel.Application.Services.Queries.GetSshAgentBundle;

/// <summary>
/// Anonymous: the SSH key agent installer bundle. It is the same public code for every server and
/// carries no secret; the installer command verifies its SHA256 against the value the administrator
/// copied from the authenticated admin panel.
/// </summary>
public sealed record GetSshAgentBundleQuery;
