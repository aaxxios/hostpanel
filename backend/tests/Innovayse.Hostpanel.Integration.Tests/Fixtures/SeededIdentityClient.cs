using Innovayse.Hostpanel.Domain.Clients;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A client row linked to a real identity account.
/// </summary>
/// <param name="Client">The seeded client entity.</param>
/// <param name="Email">The email address of the linked account.</param>
public sealed record SeededIdentityClient(Client Client, string Email);
