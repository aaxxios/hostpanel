namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>What is remembered about one challenge.</summary>
internal sealed class ChallengeState
{
    /// <summary>Gets or sets the attempts taken so far.</summary>
    public int Attempts { get; set; }

    /// <summary>Gets or sets a value indicating whether a correct code already completed the challenge.</summary>
    public bool Spent { get; set; }
}
