namespace Innovayse.Hostpanel.Application.Reports.Queries.ClientStatement;

/// <summary>Full client statement result.</summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="PreviousBalance">The previous balance.</param>
/// <param name="Lines">The lines entries.</param>
/// <param name="EndingBalance">The ending balance.</param>
public record ClientStatementDto(
    int ClientId,
    string ClientName,
    decimal PreviousBalance,
    IReadOnlyList<ClientStatementLineDto> Lines,
    decimal EndingBalance);
