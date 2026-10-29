namespace Innovayse.Hostpanel.Domain.Servers.Enums;

/// <summary>
/// Determines how new accounts are distributed across servers within a group.
/// </summary>
public enum ServerFillType
{
    /// <summary>Always assign to the server with the fewest accounts.</summary>
    LeastFull = 0,

    /// <summary>Fill one server to capacity before moving to the next.</summary>
    FillUntilFull = 1,
}
