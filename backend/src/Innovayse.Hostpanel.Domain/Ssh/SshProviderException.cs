using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Domain.Ssh;

/// <summary>
/// Thrown by an SSH key provider when the server could not be reached or refused the operation.
/// <see cref="DomainException.Code"/> is the provider's machine-readable reason and is safe to
/// store on the key; the message may carry host detail and is for the server log only.
/// </summary>
public sealed class SshProviderException : DomainException
{
    /// <summary>The reason code for an unreachable or timed-out server.</summary>
    public const string Unreachable = "unreachable";

    /// <summary>Initialises the exception.</summary>
    /// <param name="code">Machine-readable reason, e.g. <c>unreachable</c>, <c>duplicate_key</c>.</param>
    /// <param name="message">Diagnostic message for the log.</param>
    public SshProviderException(string code, string message)
        : base(code, message)
    {
    }

    /// <summary>
    /// Gets whether the outcome on the server is unknown (the call may have been applied).
    /// Refusals the agent answered explicitly are definite; transport failures and timeouts are not.
    /// </summary>
    public bool IsOutcomeUnknown => Code is Unreachable or "internal_error" or "timeout";
}
