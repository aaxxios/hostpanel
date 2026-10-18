namespace Innovayse.Hostpanel.Application.Reports.Queries.TopClientsByIncome;

/// <summary>Query for the Top 10 Clients by Income (transaction-based) report.</summary>
/// <param name="Take">How many clients to return, ranked by income, highest first.</param>
public sealed record TopClientsByIncomeQuery(int Take = 10);
