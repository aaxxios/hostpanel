namespace Innovayse.Hostpanel.Domain.Migration;

/// <summary>How many records of one entity type a running pull has processed so far, by outcome.</summary>
/// <param name="Imported">Records created here.</param>
/// <param name="Skipped">Records deliberately left alone, for example because they already existed.</param>
/// <param name="Failed">Records whose import threw an error and wrote nothing.</param>
public sealed record MigrationEntityCounts(int Imported, int Skipped, int Failed);
