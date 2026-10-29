namespace Innovayse.Providers.CWP.Models.Enums;

/// <summary>How CWP answered a request to change an account.</summary>
public enum CwpAccountOutcome
{
    /// <summary>CWP applied the change.</summary>
    Succeeded = 0,

    /// <summary>
    /// CWP refused to create the account because one already exists under that username,
    /// typically from an earlier attempt whose answer was lost.
    /// </summary>
    AlreadyExists = 1,

    /// <summary>CWP refused, and the account is already in the state the request asked for.</summary>
    AlreadyInState = 2,

    /// <summary>CWP refused and the account is not in the requested state.</summary>
    Refused = 3,
}
