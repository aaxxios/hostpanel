namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>Which page of which kind of record a migration pull asks the legacy source for.</summary>
/// <param name="DataSet">
/// The kind of record, in the source's own vocabulary (<c>clients</c>, <c>invoices</c>,
/// <c>ticket_replies</c> and so on). The source decides the spelling; the migration only passes it through.
/// </param>
/// <param name="Page">The page to fetch, counting from 1.</param>
/// <param name="PerPage">The most records the page may hold.</param>
/// <param name="AfterId">
/// The source row id the page starts after (keyset paging): the page holds the next records in id
/// order whose id is greater. A source that pages by keyset ignores <paramref name="Page"/>; one
/// that does not ignores this and answers by page number.
/// </param>
public sealed record MigrationPageSelection(string DataSet, int Page, int PerPage, long AfterId);
