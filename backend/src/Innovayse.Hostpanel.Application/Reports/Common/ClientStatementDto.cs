namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>Full client statement result. Amounts of different currencies are never added together.</summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="Balances">One opening and closing balance per currency the client has activity in, ordered by currency code.</param>
/// <param name="Lines">The lines entries, each in its own currency.</param>
public record ClientStatementDto(
    int ClientId,
    string ClientName,
    IReadOnlyList<ClientStatementBalanceDto> Balances,
    IReadOnlyList<ClientStatementLineDto> Lines);
