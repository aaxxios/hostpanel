namespace Innovayse.Hostpanel.Application.Reports.Queries.ClientStatement;

/// <summary>Query for a single client's account statement.</summary>
/// <param name="ClientId">The client to statement.</param>
/// <param name="From">Earliest activity to include, or <see langword="null"/> for no lower bound.</param>
/// <param name="To">Latest activity to include, or <see langword="null"/> for up to now.</param>
public sealed record ClientStatementQuery(
    int ClientId,
    DateOnly? From = null,
    DateOnly? To = null);
