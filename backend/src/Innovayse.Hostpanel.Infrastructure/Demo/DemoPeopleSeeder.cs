using System.Security.Cryptography;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Seeds the people: the published demo administrator, the published demo client with a company
/// and a billing contact, and a few more fictional customers so lists, charts and reports have
/// more than one row. Accounts are created through Identity (so the stored hash is the real one)
/// and clients through <see cref="Client.Create"/>; an account or client that already exists is
/// left alone.
/// </summary>
internal sealed class DemoPeopleSeeder
{
    /// <summary>Prefix that guarantees upper, lower, digit and symbol characters in a generated filler password.</summary>
    private const string FillerPasswordPrefix = "Aa1!";

    /// <summary>The domain the unpublished extra customers' addresses sit under.</summary>
    private const string FillerMailDomain = "demo.example";

    /// <summary>The data context the clients are added to and saved through.</summary>
    private readonly AppDbContext _db;

    /// <summary>Creates and looks up accounts.</summary>
    private readonly UserManager<AppUser> _users;

    /// <summary>The published demo accounts.</summary>
    private readonly DemoOptions _options;

    /// <summary>Creates the seeder.</summary>
    /// <param name="db">The data context the clients are added to and saved through.</param>
    /// <param name="users">Creates and looks up accounts.</param>
    /// <param name="options">The published demo accounts.</param>
    public DemoPeopleSeeder(AppDbContext db, UserManager<AppUser> users, DemoOptions options)
    {
        _db = db;
        _users = users;
        _options = options;
    }

    /// <summary>Seeds the administrator and the clients that are not there yet.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>The id of the published demo client, for the seeders that attach data to it.</returns>
    public async Task<int> SeedAsync(CancellationToken ct)
    {
        await EnsureAccountAsync(_options.AdminEmail!, _options.AdminPassword!, "Demo", "Admin", Roles.Admin);

        var demoClient = await EnsureClientAsync(
            new DemoClientProfile(
                _options.ClientEmail!, _options.ClientPassword!, "Demo", "Client", "Brightleaf Studio LLC",
                "+374 10 555 010", "15 Tumanyan Street", "Yerevan", null, "0001", "AM", "USD"),
            ct);

        (string First, string Last, string Company, string Phone, string Street, string City, string? State, string PostCode, string Country, string Currency)[] fillers =
        [
            ("Ararat", "Digital", "Ararat Digital CJSC", "+374 10 555 020", "20 Baghramyan Avenue", "Yerevan", null, "0019", "AM", "AMD"),
            ("Harper", "Lane", "Lane & Co Consulting", "+1 212 555 0130", "480 Hudson Street", "New York", "NY", "10014", "US", "USD"),
            ("Sofia", "Rinaldi", "Rinaldi Design Studio", "+39 06 555 0140", "Via Roma 12", "Rome", null, "00184", "IT", "USD"),
            ("Kenji", "Mori", "Mori Web Works", "+81 3 5550 0150", "2-4-1 Shibuya", "Tokyo", null, "150-0002", "JP", "USD"),
        ];

        foreach (var (first, last, company, phone, street, city, state, postCode, country, currency) in fillers)
        {
            var email = $"{first}.{last}@{FillerMailDomain}".ToLowerInvariant();
            await EnsureClientAsync(
                new DemoClientProfile(email, NewFillerPassword(), first, last, company, phone, street, city, state, postCode, country, currency),
                ct);
        }

        return demoClient.Id;
    }

    /// <summary>
    /// A fresh random password for an unpublished extra customer; nobody is meant to sign in as them, so it is
    /// never stored or shown, and no secret sits in the source.
    /// </summary>
    /// <returns>A random password that satisfies the identity password policy.</returns>
    private static string NewFillerPassword()
    {
        return FillerPasswordPrefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
    }

    /// <summary>Creates an account with a role when no account has the email; otherwise syncs its password to the configured one.</summary>
    /// <param name="email">Sign-in email.</param>
    /// <param name="password">Sign-in password.</param>
    /// <param name="firstName">Given name.</param>
    /// <param name="lastName">Family name.</param>
    /// <param name="role">The role to grant.</param>
    /// <returns>The existing (password synced) or the new account.</returns>
    /// <exception cref="InvalidOperationException">Identity refused the account, the password or the role.</exception>
    private async Task<AppUser> EnsureAccountAsync(string email, string password, string firstName, string lastName, string role)
    {
        var existing = await _users.FindByEmailAsync(email);
        if (existing is not null)
        {
            // Keep the account on the configured password, which may have changed since it was created.
            if (!await _users.CheckPasswordAsync(existing, password))
            {
                var token = await _users.GeneratePasswordResetTokenAsync(existing);
                var reset = await _users.ResetPasswordAsync(existing, token, password);
                if (!reset.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"The demo account {email} password could not be synced: {string.Join("; ", reset.Errors.Select(e => e.Code))}.");
                }
            }

            return existing;
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
        };

        var created = await _users.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            throw new InvalidOperationException(
                $"The demo account {email} could not be created: {string.Join("; ", created.Errors.Select(e => e.Code))}.");
        }

        var granted = await _users.AddToRoleAsync(user, role);
        if (!granted.Succeeded)
        {
            throw new InvalidOperationException(
                $"The demo account {email} could not be given the role {role}: {string.Join("; ", granted.Errors.Select(e => e.Code))}.");
        }

        return user;
    }

    /// <summary>Creates a client with its account, address and billing contact when it does not exist yet.</summary>
    /// <param name="profile">What to create.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>The existing or the new client.</returns>
    private async Task<Client> EnsureClientAsync(DemoClientProfile profile, CancellationToken ct)
    {
        var user = await EnsureAccountAsync(profile.Email, profile.Password, profile.FirstName, profile.LastName, Roles.Client);

        var existing = await _db.Clients.FirstOrDefaultAsync(c => c.UserId == user.Id, ct);
        if (existing is not null)
        {
            return existing;
        }

        var client = Client.Create(user.Id, profile.FirstName, profile.LastName, profile.Email, profile.Company);
        client.Update(profile.FirstName, profile.LastName, profile.Company, profile.Phone);
        client.UpdateAddress(profile.Street, null, profile.City, profile.State, profile.PostCode, profile.Country);
        client.SetCurrency(profile.Currency);
        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);

        client.AddContact(
            profile.FirstName, profile.LastName, profile.Company, profile.Email, profile.Phone, ContactType.Billing,
            null, null, null, null, null, null, true, true, true, true, true, true);
        await _db.SaveChangesAsync(ct);

        return client;
    }
}
