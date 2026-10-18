namespace Innovayse.Hostpanel.Application.Common;

/// <summary>
/// A person, as whichever system owns people describes them.
///
/// <para>
/// Named for the account rather than the user because this codebase already has an
/// <c>IdentityUser</c> — ASP.NET Identity's, which <c>AppUser</c> derives from. Two types
/// of that name, one of them not a person's record at all, is a trap for the next reader.
/// </para>
/// </summary>
/// <param name="Subject">
/// How the configured provider names this person: an SSO subject, or a local Identity id.
/// The only identifier the domain ever stores.
/// </param>
/// <param name="Email">The address the person signs in with.</param>
/// <param name="FirstName">Given name, empty if the provider does not hold one.</param>
/// <param name="LastName">Family name, empty if the provider does not hold one.</param>
/// <param name="TwoFactorEnabled">
/// Whether a second factor is set up. Read-only here: it is turned on and off wherever the
/// account lives, and this product only shows it.
/// </param>
/// <param name="LastLoginAt">
/// When this person last signed in to this product, or null if the provider does not
/// track it. A local deployment tracks it directly; the SSO tracks sign-ins to itself,
/// not to any one product it serves, so an SSO-backed provider leaves this null rather
/// than answering a question about a different thing.
/// </param>
/// <param name="Language">
/// The UI language this person chose, or null where the provider holds none.
///
/// A local deployment stores it on the account row and answers with it; an SSO-backed one
/// leaves it null, because the language of a person the SSO owns is the SSO's to hold and
/// hostpanel can neither read nor write it. Null therefore means "not ours to answer", never
/// "English" -- a caller that wants a default has to choose one for itself.
/// </param>
/// <param name="EmailVerified">
/// Whether the provider currently holds the address as confirmed, or null where it does not
/// answer. A local deployment reads it from the account row, so it is fresh even when the
/// caller's token was issued before the address was confirmed; an SSO-backed provider leaves it
/// null, because the SSO is the identity and its token claim is the answer.
/// </param>
public sealed record IdentityAccount(
    string Subject,
    string Email,
    string FirstName,
    string LastName,
    bool TwoFactorEnabled = false,
    DateTimeOffset? LastLoginAt = null,
    string? Language = null,
    bool? EmailVerified = null);
