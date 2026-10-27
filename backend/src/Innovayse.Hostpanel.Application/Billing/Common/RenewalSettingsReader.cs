using System.Globalization;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// Reads <see cref="BillingSettingKeys"/> into <see cref="RenewalSettings"/>. A missing or
/// unparsable row falls back to its default with a warning: a typo in System Settings must not stop
/// billing, and handlers never parse setting strings themselves.
/// </summary>
public sealed class RenewalSettingsReader
{
    /// <summary>The settings rows.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Warns about a fallback.</summary>
    private readonly ILogger<RenewalSettingsReader> _logger;

    /// <summary>Initializes the reader.</summary>
    /// <param name="context">The settings rows.</param>
    /// <param name="logger">Warns about a fallback.</param>
    public RenewalSettingsReader(IHostpanelDbContext context, ILogger<RenewalSettingsReader> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Reads the four billing settings.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The parsed settings.</returns>
    public async Task<RenewalSettings> ReadAsync(CancellationToken ct)
    {
        var keys = BillingSettingKeys.Defaults.Select(d => d.Key).ToList();
        var rows = await _context.Settings.AsNoTracking()
            .Where(s => keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, s => s.Value, ct);

        return new RenewalSettings(
            Int(rows, BillingSettingKeys.InvoiceDaysBefore),
            Days(rows),
            Bool(rows, BillingSettingKeys.AutoApplyCredit),
            Int(rows, BillingSettingKeys.SuspendAfterOverdueDays));
    }

    /// <summary>The seeded default of <paramref name="key"/>.</summary>
    /// <param name="key">A billing key.</param>
    /// <returns>Its default value.</returns>
    private static string Default(string key) => BillingSettingKeys.Defaults.First(d => d.Key == key).Value;

    /// <summary>Reads a stored value, or the default with a warning when it is missing or does not match its pattern.</summary>
    /// <param name="rows">The stored rows.</param>
    /// <param name="key">The key.</param>
    /// <returns>A value that matches the key's pattern (or is an allowed empty).</returns>
    private string Raw(IReadOnlyDictionary<string, string> rows, string key)
    {
        if (rows.TryGetValue(key, out var value)
            && ((value.Length == 0 && BillingSettingKeys.AllowsEmpty(key)) || BillingSettingKeys.Patterns[key].Regex.IsMatch(value)))
        {
            return value;
        }

        _logger.LogWarning("Billing setting {Key} is missing or invalid; using its default.", key);
        return Default(key);
    }

    /// <summary>Reads an integer key.</summary>
    /// <param name="rows">The stored rows.</param>
    /// <param name="key">The key.</param>
    /// <returns>The value.</returns>
    private int Int(IReadOnlyDictionary<string, string> rows, string key) =>
        int.Parse(Raw(rows, key), CultureInfo.InvariantCulture);

    /// <summary>Reads a boolean key.</summary>
    /// <param name="rows">The stored rows.</param>
    /// <param name="key">The key.</param>
    /// <returns>The value.</returns>
    private bool Bool(IReadOnlyDictionary<string, string> rows, string key) =>
        bool.Parse(Raw(rows, key));

    /// <summary>Reads the reminder thresholds, distinct and largest first.</summary>
    /// <param name="rows">The stored rows.</param>
    /// <returns>The thresholds.</returns>
    private IReadOnlyList<int> Days(IReadOnlyDictionary<string, string> rows) =>
        Raw(rows, BillingSettingKeys.ReminderDays)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(d => int.Parse(d, CultureInfo.InvariantCulture))
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();
}
