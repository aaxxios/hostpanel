using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Seeding;

/// <summary>
/// Adds the seeded language versions of email templates that are not stored yet. A version that is
/// already there is never touched, so an operator's edit survives every later seed.
/// </summary>
public static class EmailTemplateSeeding
{
    /// <summary>Adds each version whose slug and language pair is not stored yet.</summary>
    /// <remarks>
    /// The handlers that send these mails seed on their first use, so two of them can find the same
    /// version missing at once. The unique index on slug and language lets only one insert through;
    /// the other forgets its rejected rows and is done when every version is stored.
    /// </remarks>
    /// <param name="context">The module's data context.</param>
    /// <param name="versions">The seeded versions, one per slug and language.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every version exists.</returns>
    public static async Task AddMissingAsync(
        IHostpanelDbContext context, IReadOnlyList<EmailTemplate> versions, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(versions);

        var missing = await MissingAsync(context, versions, ct);
        if (missing.Count == 0)
        {
            return;
        }

        context.EmailTemplates.AddRange(missing);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            foreach (var version in missing)
            {
                context.EmailTemplates.Entry(version).State = EntityState.Detached;
            }

            if ((await MissingAsync(context, versions, ct)).Count > 0)
            {
                throw;
            }
        }
    }

    /// <summary>Picks the versions whose slug and language pair is not stored.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="versions">The seeded versions, one per slug and language.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The versions still missing.</returns>
    private static async Task<List<EmailTemplate>> MissingAsync(
        IHostpanelDbContext context, IReadOnlyList<EmailTemplate> versions, CancellationToken ct)
    {
        var slugs = versions.Select(v => v.Slug).Distinct().ToList();
        var stored = await context.EmailTemplates.AsNoTracking()
            .Where(t => slugs.Contains(t.Slug))
            .Select(t => new { t.Slug, t.Language })
            .ToListAsync(ct);

        return versions
            .Where(v => !stored.Any(s => s.Slug == v.Slug && s.Language == v.Language))
            .ToList();
    }

    /// <summary>Builds the three language versions of one branded template.</summary>
    /// <param name="slug">The template slug.</param>
    /// <param name="description">The staff-facing description every version carries.</param>
    /// <param name="versions">The subject and copy of each language.</param>
    /// <returns>One template per language, ready to add.</returns>
    public static IReadOnlyList<EmailTemplate> Branded(
        string slug, string description, IReadOnlyList<SeededEmailVersion> versions)
    {
        ArgumentNullException.ThrowIfNull(versions);

        return versions
            .Select(v => EmailTemplate.Create(slug, v.Copy.Language, v.Subject, SeededEmailLayout.Render(v.Copy), description))
            .ToList();
    }
}
