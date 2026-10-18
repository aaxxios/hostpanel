using FluentAssertions;
using Xunit;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// A command or query is bound straight from the HTTP edge, so no <c>*Request</c> type exists beside
/// it (rules/cqrs.md). The six local-authentication bodies are the one recorded exception.
/// </summary>
public sealed class NoRequestTypesTests
{
    /// <summary>
    /// The bodies <c>LocalAuthController</c> maps onto direct service calls (ruling 6): there is no
    /// command for them to bind to until the auth flow is converted in its own change.
    /// </summary>
    private static readonly HashSet<string> AllowedAuthRequests =
    [
        "Innovayse.Hostpanel.API.Auth.Requests.ConfirmEmailRequest",
        "Innovayse.Hostpanel.API.Auth.Requests.ForgotPasswordRequest",
        "Innovayse.Hostpanel.API.Auth.Requests.LoginRequest",
        "Innovayse.Hostpanel.API.Auth.Requests.RegisterRequest",
        "Innovayse.Hostpanel.API.Auth.Requests.ResetPasswordRequest",
        "Innovayse.Hostpanel.API.Auth.Requests.TwoFactorLoginRequest",
    ];

    /// <summary>The Application layer declares no type whose name ends in <c>Request</c>.</summary>
    [Fact]
    public void Application_DeclaresNoRequestTypes()
    {
        var offenders = Layers.AuthoredTypesOf(Layers.Application)
            .Where(type => type.Name.EndsWith("Request", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .ToList();

        offenders.Should().BeEmpty("commands and queries are bound directly; no *Request twin beside them");
    }

    /// <summary>
    /// The Domain layer declares no transport-shaped <c>*Request</c> value: provider inputs are named
    /// for the instruction or order they carry. A persisted entity that is a business noun
    /// (<c>CancellationRequest</c>, a thing a client files) is not such a value and is exempt.
    /// </summary>
    [Fact]
    public void Domain_DeclaresNoRequestValueTypes()
    {
        var offenders = Layers.AuthoredTypesOf(Layers.Domain)
            .Where(type => type.Name.EndsWith("Request", StringComparison.Ordinal))
            .Where(type => !typeof(Innovayse.Hostpanel.SharedKernel.Domain.BaseEntity).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToList();

        offenders.Should().BeEmpty("name provider inputs for the instruction or order they carry, not *Request");
    }

    /// <summary>The API layer declares no <c>*Request</c> type outside the explicit allow-list.</summary>
    [Fact]
    public void Api_DeclaresNoRequestTypesOutsideTheAllowList()
    {
        var offenders = Layers.AuthoredTypesOf(Layers.Api)
            .Where(type => type.Name.EndsWith("Request", StringComparison.Ordinal))
            .Select(type => type.FullName!)
            .Where(name => !AllowedAuthRequests.Contains(name))
            .ToList();

        offenders.Should().BeEmpty("only the six local-authentication bodies may remain");
    }

    /// <summary>The allow-list is not stale: each entry still exists, so it cannot hide a future type.</summary>
    [Fact]
    public void AllowList_ContainsOnlyTypesThatStillExist()
    {
        var existing = Layers.AuthoredTypesOf(Layers.Api).Select(type => type.FullName!).ToHashSet();

        AllowedAuthRequests.Except(existing).Should().BeEmpty("delete an allow-list entry when its type goes");
    }
}
