namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>What the gateway reconciler knows about one invoice's latest hosted gateway session before it asks the gateway.</summary>
/// <param name="InvoiceId">The invoice the session was opened for.</param>
/// <param name="ClientId">The client the invoice belongs to.</param>
/// <param name="Module">The payment plugin id that opened the session.</param>
/// <param name="StartedAt">When the session was registered with the gateway, UTC.</param>
/// <param name="CheckedAt">When the reconciler last asked the gateway about it, UTC; null when it has not yet.</param>
/// <param name="CheckCount">How many times the reconciler has asked the gateway about it.</param>
/// <param name="Settled">Whether the invoice was already settled by another path, so the session is only swept for late money.</param>
public sealed record GatewaySessionCheck(
    int InvoiceId,
    int ClientId,
    string Module,
    DateTimeOffset StartedAt,
    DateTimeOffset? CheckedAt,
    int CheckCount,
    bool Settled);
