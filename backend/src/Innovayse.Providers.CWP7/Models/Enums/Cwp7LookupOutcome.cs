namespace Innovayse.Providers.CWP7.Models.Enums;

/// <summary>What asking CWP7 about one account established.</summary>
internal enum Cwp7LookupOutcome
{
    /// <summary>The account exists; its details are available.</summary>
    Found = 0,

    /// <summary>CWP7 answered successfully and holds no such account.</summary>
    NotFound = 1,

    /// <summary>
    /// The answer could not be used: a refusal, an error page or an unrecognised shape. Nothing
    /// is known about the account, so no caller may read this as "absent".
    /// </summary>
    Unknown = 2,
}
