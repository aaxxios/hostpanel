using Innovayse.Hostpanel.Domain.Auth.Enums;

namespace Innovayse.Hostpanel.Domain.Auth;

/// <summary>
/// One authorization fact about one subject: this person holds this role here.
///
/// <para>
/// Deliberately not a user record. It carries no email, no name and no password,
/// nothing an identity provider owns, so the same table serves a deployment whose people
/// live in an external identity provider and one whose people live in <c>AspNetUsers</c>.
/// The subject is whatever the configured provider calls a person.
/// </para>
///
/// <para>
/// Roles stay here rather than moving to the identity provider because <see cref="Role.Reseller"/>
/// and <see cref="Role.Client"/> are this product's vocabulary, a hosting business's, and
/// because someone running this against a provider they do not own could not add them to it.
/// Keeping them local is also what lets one authorization path serve both modes instead of two.
/// </para>
/// </summary>
public sealed class SubjectRole
{
    /// <summary>The person the role applies to, as the configured identity provider names them.</summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>The role the subject holds.</summary>
    public Role Role { get; private set; }

    /// <summary>EF Core materialization only.</summary>
    private SubjectRole()
    {
    }

    /// <summary>Grants <paramref name="role"/> to <paramref name="subject"/>. The only way a grant comes into existence.</summary>
    /// <param name="subject">The subject receiving the role; not blank.</param>
    /// <param name="role">The role granted.</param>
    /// <returns>The new grant.</returns>
    /// <exception cref="ArgumentException"><paramref name="subject"/> is null, empty or whitespace.</exception>
    public static SubjectRole Grant(string subject, Role role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        return new SubjectRole
        {
            Subject = subject,
            Role = role,
        };
    }
}
