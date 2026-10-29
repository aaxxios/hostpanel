using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Domain.Ssh;

/// <summary>
/// Thrown by an SSH key provider when the server could not be reached or refused the operation.
/// <see cref="Failure"/> is the typed reason and is safe to store on the key; the message may
/// carry host detail and is for the server log only.
/// </summary>
public sealed class SshProviderException : DomainException
{
    /// <summary>
    /// The error code this exception carries as <see cref="DomainException.Code"/>; the same value
    /// as <c>HostpanelErrorCodes.SSH_SERVER_UNAVAILABLE</c>, which Domain may not reference.
    /// </summary>
    public const string ErrorCode = "SSH_SERVER_UNAVAILABLE";

    /// <summary>Initialises the exception.</summary>
    /// <param name="failure">Why the call failed.</param>
    /// <param name="message">Diagnostic message for the log.</param>
    public SshProviderException(SshProviderFailure failure, string message)
        : base(ErrorCode, message)
    {
        Failure = failure;
    }

    /// <summary>Gets why the provider call failed.</summary>
    public SshProviderFailure Failure { get; }

    /// <summary>
    /// Gets whether the outcome on the server is unknown (the call may have been applied).
    /// Refusals the agent answered explicitly are definite; transport failures and timeouts are not.
    /// </summary>
    public bool IsOutcomeUnknown =>
        Failure is SshProviderFailure.Unreachable or SshProviderFailure.InternalError or SshProviderFailure.Timeout;
}
