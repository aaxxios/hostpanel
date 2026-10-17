using FluentAssertions;
using Xunit;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Identity never travels in a body: the handler reads it from <c>ICurrentUserService</c>. A command or
/// query carrying <c>UserId</c> or <c>TenantId</c> invites the controller to copy it from the client.
/// </summary>
public sealed class NoIdentityOnMessagesTests
{
    /// <summary>The property names that smuggle the caller's identity into a message.</summary>
    private static readonly string[] ForbiddenNames = ["UserId", "TenantId"];

    /// <summary>No command or query record exposes a <c>UserId</c> or <c>TenantId</c> property.</summary>
    [Fact]
    public void CommandsAndQueries_CarryNoUserIdOrTenantId()
    {
        var offenders = Layers.AuthoredTypesOf(Layers.Application)
            .Where(type => type.IsClass
                && (type.Name.EndsWith("Command", StringComparison.Ordinal)
                    || type.Name.EndsWith("Query", StringComparison.Ordinal)))
            .SelectMany(type => type.GetProperties()
                .Where(property => ForbiddenNames.Contains(property.Name))
                .Select(property => $"{type.FullName}.{property.Name}"))
            .ToList();

        offenders.Should().BeEmpty("the handler resolves identity from ICurrentUserService, not from the message");
    }
}
