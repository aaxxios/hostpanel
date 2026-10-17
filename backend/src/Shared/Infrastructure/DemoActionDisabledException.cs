using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// A restricted command reached the bus on a demo deployment. A <see cref="DomainException"/> so the
/// exception middleware answers it with the localised sentence for its code and the status
/// <c>StatusMapping</c> gives it (403), exactly as it already does for a failed validation.
/// </summary>
public sealed class DemoActionDisabledException : DomainException
{
    /// <summary>Creates the refusal for a command type.</summary>
    /// <param name="commandName">The refused command's type name, for the log only.</param>
    public DemoActionDisabledException(string commandName)
        : base(SharedErrorCodes.DemoActionDisabled, $"{commandName} is disabled on the demo deployment.")
    {
    }
}
