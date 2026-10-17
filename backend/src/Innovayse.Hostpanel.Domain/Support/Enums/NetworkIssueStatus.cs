namespace Innovayse.Hostpanel.Domain.Support.Enums;
/// <summary>Lifecycle status of a network issue.</summary>
public enum NetworkIssueStatus
{
    /// <summary>Issue has been reported but not yet investigated.</summary>
    Reported = 0,

    /// <summary>Issue is currently being investigated.</summary>
    Investigating = 1,

    /// <summary>Maintenance or fix has been scheduled.</summary>
    Scheduled = 2,

    /// <summary>Issue has been resolved.</summary>
    Resolved = 3,
}
