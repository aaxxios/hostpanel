using Innovayse.Hostpanel.Domain.Migration.Enums;

namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>One line of a migration job's report: what happened to one record pulled from the legacy source.</summary>
/// <param name="Id">The log row, used by the admin report as its row key.</param>
/// <param name="EntityType">The kind of record the line is about (client, invoice, ticket and so on).</param>
/// <param name="Identifier">How the legacy source names the record (an email, an invoice number), so an operator can find it there.</param>
/// <param name="Action">What the pull did with the record: imported it, skipped it, or failed on it.</param>
/// <param name="SkipReason">Why a record was skipped; null unless it was skipped. The admin report translates it.</param>
/// <param name="Reason">Technical detail of the error for a failed record; null otherwise.</param>
/// <param name="CreatedAt">When the pull handled the record (UTC).</param>
public sealed record MigrationLogDto(
    int Id,
    MigrationEntityType EntityType,
    string Identifier,
    MigrationLogAction Action,
    MigrationSkipReason? SkipReason,
    string? Reason,
    DateTimeOffset CreatedAt);
