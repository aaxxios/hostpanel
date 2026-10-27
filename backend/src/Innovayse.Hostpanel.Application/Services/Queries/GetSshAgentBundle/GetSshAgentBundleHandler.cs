using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetSshAgentBundle;

/// <summary>Handles <see cref="GetSshAgentBundleQuery"/>.</summary>
public sealed class GetSshAgentBundleHandler
{
    /// <summary>The bundle this build ships.</summary>
    private readonly ISshAgentBundle _bundle;

    /// <summary>Initialises the handler.</summary>
    /// <param name="bundle">The bundle this build ships.</param>
    public GetSshAgentBundleHandler(ISshAgentBundle bundle)
    {
        _bundle = bundle;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query.</param>
    /// <returns>The bundle as a download.</returns>
    public Result<FileDownloadDto> Handle(GetSshAgentBundleQuery query) =>
        Result<FileDownloadDto>.Success(new FileDownloadDto(
            _bundle.Content.ToArray(), "application/gzip", $"hostpanel-ssh-key-agent-{_bundle.Version}.tar.gz"));
}
