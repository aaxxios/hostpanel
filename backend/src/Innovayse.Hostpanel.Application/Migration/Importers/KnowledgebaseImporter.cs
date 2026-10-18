using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports knowledge-base categories (creating any this install does not already have, matched by
/// name), then articles deduplicated by title. Split out of <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported category or article is saved through.</param>
/// <param name="pager">Pages through the source plugin's knowledge-base datasets.</param>
/// <param name="logWriter">Writes one migration log row per article record.</param>
/// <param name="logger">Logs a skipped or failed record.</param>
public sealed class KnowledgebaseImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    ILogger<KnowledgebaseImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        // Step 1: fetch categories, create missing ones, build sourceCatId→localCategoryName map
        // Was IKbCategoryRepository.ListAllAsync -- read-only, so no tracking is needed.
        var localCats = await context.KbCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
        var catNameMap = new Dictionary<int, string>(); // sourceCatId → local category name

        await foreach (var page in pager.PagesAsync<KbCategoryRecord>(job, "knowledgebase_categories", ct))
        {
            foreach (var cat in page)
            {
                var existing = localCats.FirstOrDefault(c =>
                    string.Equals(c.Name, cat.Name, StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                {
                    catNameMap[cat.Id] = existing.Name;
                }
                else
                {
                    var newCat = KbCategory.Create(cat.Name, cat.Description, false, null);
                    // Was IKbCategoryRepository.Add.
                    context.KbCategories.Add(newCat);
                    await uow.SaveChangesAsync(ct);
                    catNameMap[cat.Id] = newCat.Name;
                }
            }
        }

        // Step 2: page through articles
        int imported = 0, skipped = 0;

        await foreach (var page in pager.PagesAsync<KbArticleRecord>(job, "knowledgebase", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var categoryName = rec.CategoryId.HasValue && catNameMap.TryGetValue(rec.CategoryId.Value, out var n) && !string.IsNullOrWhiteSpace(n)
                        ? n
                        : "General";

                    // Was IKbArticleRepository.FindByTitleAsync.
                    var existingArticle = await context.KbArticles.FirstOrDefaultAsync(a => a.Title.ToLower() == rec.Title.ToLower(), ct);
                    if (existingArticle is not null)
                    {
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Knowledgebase, rec.Title, ImportOutcome.Skipped, "Article already exists", ct);
                        skipped++;
                        continue;
                    }

                    var article = KbArticle.Create(rec.Title, rec.Content, categoryName);

                    article.Publish();

                    // Was IKbArticleRepository.Add.
                    context.KbArticles.Add(article);
                    await uow.SaveChangesAsync(ct);
                    imported++;
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Knowledgebase, rec.Title, ImportOutcome.Imported, null, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped KB article '{Title}' during migration", rec.Title);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Knowledgebase, rec.Title, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Knowledgebase, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }
}
