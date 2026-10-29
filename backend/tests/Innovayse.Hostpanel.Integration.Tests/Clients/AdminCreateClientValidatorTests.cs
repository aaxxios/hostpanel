using FluentValidation.TestHelper;
using Innovayse.Hostpanel.Application.Clients.Commands.AdminCreateClient;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Integration.Tests.Shared;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary>The rules <see cref="AdminCreateClientValidator"/> adds, without a database.</summary>
public sealed class AdminCreateClientValidatorTests
{
    /// <summary>Validator instance under test.</summary>
    private readonly AdminCreateClientValidator _validator = new(new EchoLocalizer<ApplicationValidationMessages>());

    /// <summary>Builds a command that links an existing account, with the fields under test overridden.</summary>
    /// <param name="status">The status to put on the command.</param>
    /// <param name="language">The language to put on the command.</param>
    /// <param name="email">The email to put on the command.</param>
    /// <param name="street">The street to put on the command.</param>
    /// <returns>The command.</returns>
    private static AdminCreateClientCommand Command(
        ClientStatus? status = null, string? language = null, string? email = null, string? street = null) => new(
            CreateNewUser: false, ExistingUserId: "user-1", Email: email,
            Password: null, FirstName: "Ada", LastName: "Lovelace", CompanyName: null, Phone: null,
            Address: new PostalAddress(street, null, null, null, null, null),
            Preferences: new ClientPreferencesDto(language, null, null, null, null), Status: status);

    /// <summary>A command with only valid optional fields passes.</summary>
    [Fact]
    public void ValidCommand_ShouldPass()
    {
        _validator.TestValidate(Command(status: ClientStatus.Suspended, language: "en", email: "ada@example.test"))
            .ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>A number the JSON binder accepts but that names no member of the enum fails.</summary>
    /// <param name="value">A number that is not Active, Inactive, Suspended or Closed.</param>
    [Theory]
    [InlineData(99)]
    [InlineData(-1)]
    public void UndefinedStatus_ShouldFail(int value)
    {
        _validator.TestValidate(Command(status: (ClientStatus)value)).ShouldHaveValidationErrorFor(x => x.Status);
    }

    /// <summary>No status passes: the client starts Active.</summary>
    [Fact]
    public void NoStatus_ShouldPass()
    {
        _validator.TestValidate(Command(status: null)).ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    /// <summary>A language outside the supported list fails.</summary>
    [Fact]
    public void UnsupportedLanguage_ShouldFail()
    {
        _validator.TestValidate(Command(language: "xx")).ShouldHaveValidationErrorFor(x => x.Preferences.Language);
    }

    /// <summary>An address that is malformed fails even when linking an existing account.</summary>
    [Fact]
    public void MalformedEmail_ShouldFail()
    {
        _validator.TestValidate(Command(email: "not-an-email")).ShouldHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>A street longer than its column fails instead of reaching the database.</summary>
    [Fact]
    public void OverlongStreet_ShouldFail()
    {
        _validator.TestValidate(Command(street: new string('s', 201))).ShouldHaveValidationErrorFor(x => x.Address.Street);
    }
}
