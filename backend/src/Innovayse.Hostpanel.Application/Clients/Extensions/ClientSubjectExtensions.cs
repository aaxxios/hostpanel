using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Extensions;

/// <summary>What makes a person eligible to hold a place on a client account.</summary>
public static class ClientSubjectExtensions
{
    /// <summary>
    /// Whether the subject is a customer account: a person the identity provider knows who holds the
    /// <see cref="Role.Client"/> role. Staff are people too, but a staff account is not one a client
    /// account may be shared with or handed to.
    /// </summary>
    /// <param name="identity">Reads the person, wherever people live.</param>
    /// <param name="roles">Reads the roles the person holds.</param>
    /// <param name="subject">The person's subject.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when the person exists and holds the client role.</returns>
    public static async Task<bool> IsClientAccountAsync(
        this IIdentityProvider identity, ISubjectRoleStore roles, string subject, CancellationToken ct)
    {
        var account = await identity.FindBySubjectAsync(subject, ct);
        return account is not null && await roles.HoldsAsync(subject, Role.Client, ct);
    }
}
