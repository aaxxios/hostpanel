using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Support;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports download categories, then downloadable files placed into the imported category (or the
/// first available one, or none). Split out of <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported category or file is saved through.</param>
/// <param name="pager">Pages through the source plugin's download datasets.</param>
/// <param name="logWriter">Writes one migration log row per file record.</param>
/// <param name="logger">Logs a failed record.</param>
public sealed class DownloadsImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    ILogger<DownloadsImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        // Step 1: categories
        var catIdMap = new Dictionary<int, int>(); // sourceCatId → localCatId

        await foreach (var page in pager.PagesAsync<DownloadCategoryRecord>(job, "download_categories", ct))
        {
            foreach (var cat in page)
            {
                var newCat = DownloadCategory.Create(cat.Name, cat.Description ?? string.Empty, cat.Hidden, null);
                // Was IDownloadRepository.AddCategory.
                context.DownloadCategories.Add(newCat);
                await uow.SaveChangesAsync(ct);
                catIdMap[cat.Id] = newCat.Id;
            }
        }

        // Step 2: files
        int imported = 0, skipped = 0;

        await foreach (var page in pager.PagesAsync<DownloadRecord>(job, "downloads", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var catId = rec.CategoryId.HasValue && catIdMap.TryGetValue(rec.CategoryId.Value, out var localCatId)
                        ? localCatId
                        : catIdMap.Values.FirstOrDefault();

                    var resolvedTitle = !string.IsNullOrWhiteSpace(rec.Title) ? rec.Title
                        : !string.IsNullOrWhiteSpace(rec.Filename) ? rec.Filename
                        : "Untitled";

                    var download = Download.Create(
                        resolvedTitle,
                        rec.Description ?? string.Empty,
                        rec.Type,
                        rec.Filename,
                        catId,
                        rec.ClientsOnly,
                        false,
                        rec.Hidden);

                    // Was IDownloadRepository.Add.
                    context.Downloads.Add(download);
                    await uow.SaveChangesAsync(ct);
                    imported++;
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Downloads, rec.Title, ImportOutcome.Imported, null, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped download '{Title}' during migration", rec.Title);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Downloads, rec.Title, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Downloads, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }
}
