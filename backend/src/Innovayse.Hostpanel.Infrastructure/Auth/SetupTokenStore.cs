using System.Security.Cryptography;
using System.Text;
using Innovayse.Hostpanel.Application.Auth.Seeding;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// EF Core implementation of <see cref="ISetupTokenStore"/> over the setting row
/// <see cref="SetupTokenSeeder"/> issues.
/// </summary>
public sealed class SetupTokenStore : ISetupTokenStore
{
    /// <summary>The application DbContext holding the setting row.</summary>
    private readonly AppDbContext _db;

    /// <summary>Encrypts the token at rest and decrypts it for the comparison.</summary>
    private readonly ISecretSettingCipher _cipher;

    /// <summary>Initializes a new instance of the <see cref="SetupTokenStore"/> class.</summary>
    /// <param name="db">The application DbContext holding the setting row.</param>
    /// <param name="cipher">Encrypts the token at rest and decrypts it for the comparison.</param>
    public SetupTokenStore(AppDbContext db, ISecretSettingCipher cipher)
    {
        _db = db;
        _cipher = cipher;
    }

    /// <inheritdoc/>
    public async Task<string> GetOrIssueAsync(string candidate, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(candidate);

        // Tracked, not AsNoTracking: an existing row is updated and saved further down.
        var existing = await _db.Settings.FirstOrDefaultAsync(s => s.Key == SetupTokenSeeder.SettingKey, ct);
        if (existing is not null && !string.IsNullOrWhiteSpace(existing.Value))
        {
            return _cipher.Reveal(existing.Value);
        }

        // The token is a secret setting, so it is stored encrypted like every other one.
        var stored = _cipher.Protect(candidate);
        if (existing is null)
        {
            _db.Settings.Add(Setting.Create(SetupTokenSeeder.SettingKey, stored, SetupTokenSeeder.SettingDescription));
        }
        else
        {
            // The row is blank on exactly one path: setup was completed and then the Admin role
            // was revoked from every holder, so the installation needs a way back in.
            existing.UpdateValue(stored);
        }

        await _db.SaveChangesAsync(ct);
        return candidate;
    }

    /// <inheritdoc/>
    public async Task<bool> TryConsumeAsync(string? presented, CancellationToken ct)
    {
        // Untracked: the row is retired by the conditional update below, never by a tracked save.
        var issued = await _db.Settings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == SetupTokenSeeder.SettingKey, ct);

        // Fails closed. A missing or blank row means either that this installation has already
        // been bootstrapped through the gate, or that the boot-time issuer never ran, and in
        // neither case is granting Admin to whoever asked the safe reading.
        if (issued is null || string.IsNullOrWhiteSpace(issued.Value) || string.IsNullOrEmpty(presented))
        {
            return false;
        }

        // Fixed-time comparison. The tokens are 256-bit random, so a timing oracle is not the
        // realistic attack here, but a byte-by-byte early exit on the one secret that guards
        // ownership of the installation is not worth defending in review.
        var matches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(_cipher.Reveal(issued.Value)),
            Encoding.UTF8.GetBytes(presented));
        if (!matches)
        {
            return false;
        }

        // Retired by blanking rather than deleting: an empty value is refused above exactly as a
        // missing row is, and the row left behind records that this installation was
        // bootstrapped through the gate. Saved here rather than after the grant so that a failure
        // between the two leaves the token spent rather than the role granted and the token live.
        //
        // One conditional statement, not a read then a save: it blanks the row only while it still
        // holds the token that was matched, so of two requests presenting the same token at once
        // exactly one changes the row and wins; the other changes nothing and is refused.
        // The stored (encrypted) form is what the row holds, so the condition compares that.
        var token = issued.Value;
        var retired = await _db.Settings
            .Where(s => s.Key == SetupTokenSeeder.SettingKey && s.Value == token)
            .ExecuteUpdateAsync(set => set.SetProperty(s => s.Value, string.Empty), ct);
        return retired == 1;
    }
}
