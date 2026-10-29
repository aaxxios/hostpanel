namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>What <c>IHostingProbe</c> is asked to read.</summary>
/// <param name="Domain">The domain name to probe. Client-supplied text: the probe refuses anything that is not a public host name.</param>
public sealed record HostingProbeInput(string Domain);
