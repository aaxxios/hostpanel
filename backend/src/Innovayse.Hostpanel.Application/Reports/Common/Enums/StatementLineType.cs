namespace Innovayse.Hostpanel.Application.Reports.Common.Enums;

/// <summary>What a client statement line records.</summary>
public enum StatementLineType
{
    /// <summary>An invoice raised against the client.</summary>
    Invoice,

    /// <summary>A transaction recorded for the client.</summary>
    Transaction,

    /// <summary>
    /// The payment of an invoice that arrived already paid (imported history): the money was taken
    /// where the history comes from, so no transaction was recorded here.
    /// </summary>
    PaidHistory,
}
