namespace Innovayse.Hostpanel.Application.Support.Common.Enums;

/// <summary>An action a bulk ticket update applies to every selected ticket. Sent on the wire in any case.</summary>
public enum BulkTicketAction
{
    /// <summary>Close the ticket.</summary>
    Close = 0,

    /// <summary>Delete the ticket.</summary>
    Delete = 1,

    /// <summary>Flag the ticket.</summary>
    Flag = 2,

    /// <summary>Remove the flag from the ticket.</summary>
    Unflag = 3,
}
