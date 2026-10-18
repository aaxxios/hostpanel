namespace Innovayse.Hostpanel.Application.Reports.Queries.ClientStatement;

/// <summary>One line in a client statement.</summary>
/// <param name="Type">The type.</param>
/// <param name="Date">The date.</param>
/// <param name="Description">The description.</param>
/// <param name="Debit">The debit.</param>
/// <param name="Credit">The credit.</param>
/// <param name="Balance">The balance.</param>
public record ClientStatementLineDto(
    string Type,
    string Date,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal Balance);
