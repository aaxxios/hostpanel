namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// A command or query is bound straight from the HTTP edge, so no <c>*Request</c> type exists beside
/// it (rules/cqrs.md).
/// </summary>
public sealed class NoRequestTypesTests
{
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

    /// <summary>The API layer declares no <c>*Request</c> type.</summary>
    [Fact]
    public void Api_DeclaresNoRequestTypes()
    {
        var offenders = Layers.AuthoredTypesOf(Layers.Api)
            .Where(type => type.Name.EndsWith("Request", StringComparison.Ordinal))
            .Select(type => type.FullName!)
            .ToList();

        offenders.Should().BeEmpty("controllers bind the command directly; no *Request body exists beside it");
    }
}
