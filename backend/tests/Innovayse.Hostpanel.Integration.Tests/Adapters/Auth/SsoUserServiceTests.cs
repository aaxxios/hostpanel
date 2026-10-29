using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Auth.Common.Enums;
using Innovayse.Hostpanel.Infrastructure.Auth;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Auth;

/// <summary>
/// The sign-on mode stand-in for the local account store: it has no accounts, so every read answers the
/// port's "no such account" value and every write throws, without Identity being registered at all.
/// </summary>
public sealed class SsoUserServiceTests
{
    /// <summary>An account id a caller might pass; nothing exists under it in this mode.</summary>
    private const string UserId = "user-1";

    /// <summary>The adapter under test; it holds no state, so one instance serves every test.</summary>
    private readonly SsoUserService _users = new();

    /// <summary>A password sign-in fails as for an unknown account, carrying no account.</summary>
    /// <returns>A task that completes when the assertions ran.</returns>
    [Fact]
    public async Task SignInWithPasswordAsync_FailsWithNoAccountAsync()
    {
        var result = await _users.SignInWithPasswordAsync(
            new PasswordSignInInput("person@example.com", "correct horse battery"), CancellationToken.None);

        result.Outcome.Should().Be(PasswordSignInOutcome.Failed);
        result.User.Should().BeNull();
    }

    /// <summary>Every read answers as for an account that does not exist.</summary>
    /// <returns>A task that completes when the assertions ran.</returns>
    [Fact]
    public async Task Reads_AnswerAsForAnUnknownAccountAsync()
    {
        var ct = CancellationToken.None;

        (await _users.FindByIdAsync(UserId, ct)).Should().BeNull();
        (await _users.IsEmailConfirmedAsync(UserId, ct)).Should().BeFalse();
        (await _users.IsTwoFactorEnabledAsync(UserId, ct)).Should().BeFalse();
        (await _users.EnableTwoFactorAsync(UserId, "123456", ct)).Should().BeFalse();
        (await _users.VerifyTwoFactorCodeAsync(UserId, "123456", ct)).Should().BeFalse();
        (await _users.CheckPasswordAsync(UserId, "correct horse battery", ct)).Should().BeFalse();
        (await _users.GetSessionVersionAsync(UserId, ct)).Should().BeNull();
        (await _users.GetRolesAsync(UserId, ct)).Should().BeEmpty();
    }

    /// <summary>Creating an account throws the failure the port documents, naming the operation.</summary>
    /// <returns>A task that completes when the assertions ran.</returns>
    [Fact]
    public async Task CreateAsync_ThrowsAsync()
    {
        var act = () => _users.CreateAsync(
            new CreateUserInput("person@example.com", "correct horse battery", "Ada", "Lovelace"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("CreateAsync*");
    }

    /// <summary>Every other write throws too: there is no account to write to.</summary>
    /// <returns>A task that completes when the assertions ran.</returns>
    [Fact]
    public async Task Writes_ThrowAsync()
    {
        var ct = CancellationToken.None;

        await FluentActions.Awaiting(() => _users.AddToRoleAsync(UserId, "Client", ct))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("AddToRoleAsync*");
        await FluentActions.Awaiting(() => _users.UpdateLastLoginAsync(UserId, ct))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("UpdateLastLoginAsync*");
        await FluentActions.Awaiting(() => _users.GenerateTwoFactorSecretAsync(UserId, ct))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("GenerateTwoFactorSecretAsync*");
        await FluentActions.Awaiting(() => _users.DisableTwoFactorAsync(UserId, ct))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("DisableTwoFactorAsync*");
    }
}
