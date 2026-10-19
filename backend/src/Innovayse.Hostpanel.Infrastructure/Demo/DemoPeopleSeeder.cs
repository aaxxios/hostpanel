using System.Security.Cryptography;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Seeds the people: the published demo administrator and client, staff with different roles, the demo
/// client's full profile (contacts, team members, invitations, a saved-card customer and a credit
/// balance), and fourteen more fictional customers in mixed states so lists, charts and reports have
/// something to show. Accounts are created through Identity (so the stored hash is the real one), the
/// role of each is also granted in the subject-role table the authorization reads, and clients are built
/// through <see cref="Client.Create"/> and its behaviour methods; an account or client that already exists
/// is left alone.
/// </summary>
internal sealed class DemoPeopleSeeder
{
    /// <summary>Prefix that guarantees upper, lower, digit and symbol characters in a generated password.</summary>
    private const string GeneratedPasswordPrefix = "Aa1!";

    /// <summary>The domain the unpublished accounts' addresses sit under.</summary>
    private const string FillerMailDomain = "demo.example";

    /// <summary>Everything the demo client's team member may see except money and account settings.</summary>
    private const ClientPermission OperationsPermissions =
        ClientPermission.ViewProductsServices | ClientPermission.ViewDomains | ClientPermission.ManageDomainSettings
        | ClientPermission.ViewOpenSupportTickets | ClientPermission.ViewEmails;

    /// <summary>What the demo client's finance team member may do.</summary>
    private const ClientPermission FinancePermissions =
        ClientPermission.ViewPayInvoices | ClientPermission.ViewAcceptQuotes | ClientPermission.ViewEmails;

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

    /// <summary>Seeds the administrator, the staff and the clients that are not there yet.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>The id of the published demo client, for the seeders that attach data to it.</returns>
    public async Task<int> SeedAsync(CancellationToken ct)
    {
        await EnsureAccountAsync(_options.AdminEmail!, _options.AdminPassword!, "Demo", "Admin", Roles.Admin, ct);
        await SeedStaffAsync(ct);

        var (demoClient, demoCreated) = await EnsureClientAsync(
            new DemoClientProfile(
                _options.ClientEmail!, _options.ClientPassword!, "Demo", "Client", "Brightleaf Studio LLC",
                "+1 555 0110", "15 Example Street", "Northvale", null, "00001", "US", "USD",
                ClientStatus.Active, 120m),
            ct);
        if (demoCreated)
        {
            await CompleteDemoClientAsync(demoClient, ct);
        }

        (string First, string Last, string Company, string Phone, string Street, string City, string? State, string PostCode, string Country, string Currency, ClientStatus Status, decimal Credit)[] fillers =
        [
            ("Maple", "Digital", "Maple Digital Inc", "+1 555 0120", "20 Sample Avenue", "Northvale", null, "00019", "US", "AMD", ClientStatus.Active, 0m),
            ("Harper", "Lane", "Lane & Co Consulting", "+1 212 555 0130", "480 Hudson Street", "New York", "NY", "10014", "US", "USD", ClientStatus.Active, 40m),
            ("Sofia", "Rinaldi", "Rinaldi Design Studio", "+39 06 555 0140", "Via Roma 12", "Rome", null, "00184", "IT", "USD", ClientStatus.Active, 0m),
            ("Kenji", "Mori", "Mori Web Works", "+81 3 5550 0150", "2-4-1 Shibuya", "Tokyo", null, "150-0002", "JP", "USD", ClientStatus.Active, 0m),
            ("Nora", "Sanders", "Cloudline Systems LLC", "+1 555 0130", "7 Pine Avenue", "Northvale", null, "00010", "US", "AMD", ClientStatus.Suspended, 0m),
            ("Mark", "Brennan", "Brennan Media Group", "+44 20 7946 0140", "22 Baker Lane", "London", null, "NW1 6XE", "GB", "USD", ClientStatus.Active, 0m),
            ("Lena", "Fischer", "Fischer Analytics GmbH", "+49 30 5550 0160", "Torstrasse 101", "Berlin", null, "10119", "DE", "USD", ClientStatus.Active, 25m),
            ("Omar", "Haddad", "Haddad Logistics", "+971 4 555 0170", "Sheikh Zayed Road 18", "Dubai", null, "00000", "AE", "USD", ClientStatus.Active, 0m),
            ("Elena", "Petrova", "Petrova Interiors", "+7 495 555 0180", "Tverskaya 9", "Moscow", null, "125009", "RU", "USD", ClientStatus.Closed, 0m),
            ("Daniel", "Okafor", "Okafor Ventures Ltd", "+234 1 555 0190", "14 Marina Road", "Lagos", null, "101241", "NG", "USD", ClientStatus.Active, 0m),
            ("Anna", "Grayson", "Grayson Dental Clinic", "+1 555 0140", "3 Mill Street", "Lakeside", null, "00031", "US", "AMD", ClientStatus.Active, 15000m),
            ("Tomas", "Novak", "Novak Craft Brewery", "+420 2 5550 0200", "Narodni 25", "Prague", null, "11000", "CZ", "USD", ClientStatus.Suspended, 0m),
            ("Isabel", "Costa", "Costa Travel Agency", "+351 21 555 0210", "Rua Augusta 100", "Lisbon", null, "1100-053", "PT", "USD", ClientStatus.Active, 0m),
            ("Maya", "Patel", "Patel Software Pvt", "+91 22 5550 0220", "Linking Road 5", "Mumbai", "MH", "400050", "IN", "USD", ClientStatus.Active, 0m),
        ];

        for (var i = 0; i < fillers.Length; i++)
        {
            var (first, last, company, phone, street, city, state, postCode, country, currency, status, credit) = fillers[i];
            var email = $"{first}.{last}@{FillerMailDomain}".ToLowerInvariant();
            var (client, created) = await EnsureClientAsync(
                new DemoClientProfile(
                    email, NewGeneratedPassword(), first, last, company, phone, street, city, state, postCode, country, currency,
                    status, credit),
                ct);

            if (created)
            {
                await CompleteFillerAsync(client, i, ct);
            }
        }

        return demoClient.Id;
    }

    /// <summary>
    /// A fresh random password for an account nobody is meant to sign in as; it is never stored or shown,
    /// and no secret sits in the source. A guessable password here would be a hole in a public demo.
    /// </summary>
    /// <returns>A random password that satisfies the identity password policy.</returns>
    private static string NewGeneratedPassword()
    {
        return GeneratedPasswordPrefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
    }

    /// <summary>Creates the staff accounts: two administrators and two resellers.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the accounts exist.</returns>
    private async Task SeedStaffAsync(CancellationToken ct)
    {
        (string Local, string First, string Last, string Role)[] staff =
        [
            ("support.lead", "Rafael", "Torres", Roles.Admin),
            ("billing.manager", "Julia", "Weiss", Roles.Admin),
            ("partner.north", "Peter", "Hall", Roles.Reseller),
            ("partner.south", "Mila", "Stone", Roles.Reseller),
        ];

        foreach (var (local, first, last, role) in staff)
        {
            await EnsureAccountAsync($"{local}@{FillerMailDomain}", NewGeneratedPassword(), first, last, role, ct);
        }
    }

    /// <summary>
    /// Gives the published demo client everything a full profile has: billing preferences, notification and
    /// billing settings, a card-gateway customer for the saved sample card, a credit balance, technical and
    /// general contacts, two team members with different permissions and three invitations.
    /// </summary>
    /// <param name="client">The demo client, already saved.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task CompleteDemoClientAsync(Client client, CancellationToken ct)
    {
        client.UpdatePreferences(
            PaymentMethodNames.Card, "Demo Client", "Long-standing customer; prefers invoices by email and pays by card.");
        client.UpdateNotifications(true, true, true, true, true, false);
        client.UpdateSettings(false, true, false, false, false, true, true, true);
        client.SetCardGatewayCustomerId($"demo-customer-{client.Id}");

        client.AddContact(
            "Nina", "Avery", "Brightleaf Studio LLC", $"nina.avery@{FillerMailDomain}", "+1 555 0111", ContactType.Technical,
            "15 Example Street", null, "Northvale", null, "00001", "US", true, false, true, true, true, false);
        client.AddContact(
            "Gary", "Sawyer", "Brightleaf Studio LLC", $"gary.sawyer@{FillerMailDomain}", "+1 555 0112", ContactType.General,
            "15 Example Street", null, "Northvale", null, "00001", "US", true, false, false, false, false, false);

        var editor = await EnsureAccountAsync(
            $"team.editor@{FillerMailDomain}", NewGeneratedPassword(), "Taylor", "Editor", Roles.Client, ct);
        var finance = await EnsureAccountAsync(
            $"team.finance@{FillerMailDomain}", NewGeneratedPassword(), "Lusine", "Finance", Roles.Client, ct);
        client.AddUser(editor.Id, OperationsPermissions);
        client.AddUser(finance.Id, FinancePermissions);
        await _db.SaveChangesAsync(ct);

        // One invitation already accepted (the editor above), two still waiting for an answer.
        var accepted = Invitation.Create(client.Id, editor.Email!, "Taylor", "Editor", OperationsPermissions);
        accepted.Accept();
        _db.Invitations.Add(accepted);
        _db.Invitations.Add(Invitation.Create(client.Id, $"design.lead@{FillerMailDomain}", "Mari", "Designer", ClientPermission.ViewProductsServices));
        _db.Invitations.Add(Invitation.Create(client.Id, $"auditor@{FillerMailDomain}", "Karen", "Auditor", FinancePermissions));
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Gives a filler customer the extras that vary by position: a technical contact, a team member, an invitation.</summary>
    /// <param name="client">The customer, already saved.</param>
    /// <param name="index">The customer's position in the filler list.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task CompleteFillerAsync(Client client, int index, CancellationToken ct)
    {
        if (index % 2 == 0)
        {
            client.AddContact(
                "Operations", "Team", client.CompanyName, $"ops.{index}@{FillerMailDomain}", client.Phone, ContactType.Technical,
                client.Street, null, client.City, client.State, client.PostCode, client.Country, true, false, true, true, true, false);
        }

        if (index == 1)
        {
            var member = await EnsureAccountAsync(
                $"team.lane@{FillerMailDomain}", NewGeneratedPassword(), "Owen", "Lane", Roles.Client, ct);
            client.AddUser(member.Id, ClientPermission.ViewProductsServices | ClientPermission.ViewPayInvoices);
            _db.Invitations.Add(Invitation.Create(client.Id, $"bookkeeper.lane@{FillerMailDomain}", "Ruth", "Keeper", FinancePermissions));
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Creates an account with a role when no account has the email; otherwise syncs its password to the configured one.</summary>
    /// <param name="email">Sign-in email.</param>
    /// <param name="password">Sign-in password.</param>
    /// <param name="firstName">Given name.</param>
    /// <param name="lastName">Family name.</param>
    /// <param name="role">The role to grant.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>The existing (password synced) or the new account.</returns>
    /// <exception cref="InvalidOperationException">Identity refused the account, the password or the role.</exception>
    private async Task<AppUser> EnsureAccountAsync(
        string email, string password, string firstName, string lastName, string role, CancellationToken ct)
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

            await GrantSubjectRoleAsync(existing.Id, role, ct);
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

        await GrantSubjectRoleAsync(user.Id, role, ct);
        return user;
    }

    /// <summary>Records the role in the subject-role table the authorization reads, unless the subject already holds it.</summary>
    /// <param name="subject">The account id.</param>
    /// <param name="role">The role to grant.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the row is saved.</returns>
    private async Task GrantSubjectRoleAsync(string subject, string role, CancellationToken ct)
    {
        if (await _db.SubjectRoles.AnyAsync(r => r.Subject == subject && r.Role == role, ct))
        {
            return;
        }

        _db.SubjectRoles.Add(new SubjectRole { Subject = subject, Role = role });
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Creates a client with its account, address, billing contact, state and credit when it does not exist yet.</summary>
    /// <param name="profile">What to create.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>The client, and whether this call created it.</returns>
    private async Task<(Client Client, bool Created)> EnsureClientAsync(DemoClientProfile profile, CancellationToken ct)
    {
        var user = await EnsureAccountAsync(profile.Email, profile.Password, profile.FirstName, profile.LastName, Roles.Client, ct);

        var existing = await _db.Clients.FirstOrDefaultAsync(c => c.UserId == user.Id, ct);
        if (existing is not null)
        {
            return (existing, false);
        }

        var client = Client.Create(user.Id, profile.FirstName, profile.LastName, profile.Email, profile.Company);
        client.Update(profile.FirstName, profile.LastName, profile.Company, profile.Phone);
        client.UpdateAddress(profile.Street, null, profile.City, profile.State, profile.PostCode, profile.Country);
        client.SetCurrency(profile.Currency);
        if (profile.Credit > 0)
        {
            client.AddCredit(profile.Credit);
        }

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);

        client.AddContact(
            profile.FirstName, profile.LastName, profile.Company, profile.Email, profile.Phone, ContactType.Billing,
            null, null, null, null, null, null, true, true, true, true, true, true);

        // The state is applied last: a suspended or closed account takes no further changes of ours.
        if (profile.Status == ClientStatus.Suspended)
        {
            client.Suspend();
        }
        else if (profile.Status == ClientStatus.Closed)
        {
            client.Close();
        }

        await _db.SaveChangesAsync(ct);

        return (client, true);
    }
}
