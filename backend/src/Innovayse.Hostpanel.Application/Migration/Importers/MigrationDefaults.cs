namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Names an import writes when the source left a required value empty. Kept in one place so
/// every importer falls back to the same words and an operator can find and rename them afterwards.
/// </summary>
public static class MigrationDefaults
{
    /// <summary>Knowledge base category an article goes into when the source names no category.</summary>
    public const string KnowledgebaseCategoryName = "General";

    /// <summary>Title a download gets when the source gives neither a title nor a file name.</summary>
    public const string DownloadTitle = "Untitled";

    /// <summary>
    /// Reference an imported paid invoice's payment is recorded under. The source sends no gateway
    /// reference for an invoice, so the payment history says where it came from instead.
    /// </summary>
    public const string PaidInvoiceReference = "migrated";
}
