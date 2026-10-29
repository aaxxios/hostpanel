using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// A test host whose clock the session renewal tests move by hand.
/// </summary>
/// <param name="Client">The client calling the host.</param>
/// <param name="Clock">The clock the host reads.</param>
/// <param name="Jwt">The host's token service.</param>
/// <param name="Services">The host's services, for the account a token is minted for.</param>
public sealed record SessionRenewalHost(HttpClient Client, ManualTimeProvider Clock, IJwtService Jwt, IServiceProvider Services);
