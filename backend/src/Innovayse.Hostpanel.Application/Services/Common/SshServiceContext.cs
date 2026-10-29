using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;

namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>
/// Everything resolved server-side for one SSH request: the caller's service, the server it is
/// assigned to, that server's SSH settings, the provider bound to it, and the account. Built only
/// by <see cref="ISshServiceContextResolver"/>; nothing in it comes from the request body.
/// </summary>
/// <param name="Service">The caller's service.</param>
/// <param name="State">The access state as far as could be determined without calling the server.</param>
/// <param name="Target">The assigned server, when any.</param>
/// <param name="Agent">Its SSH settings, when enabled and complete.</param>
/// <param name="Provider">The provider, when the module is supported and settings are usable.</param>
/// <param name="Account">The hosting account, when the service has one.</param>
public sealed record SshServiceContext(
    ClientService Service,
    SshAccessState State,
    ServerSshTargetDto? Target,
    ServerSshAgentEndpoint? Agent,
    ISshKeyProvider? Provider,
    SshAccount? Account);
