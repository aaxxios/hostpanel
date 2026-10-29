namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>
/// Thrown by an <see cref="Innovayse.Hostpanel.Application.Common.Interfaces.IMigrationSource"/> when the
/// job's source address is not one this server may call (a scheme that is not allowed, or a host that
/// resolves only to internal addresses). The pull records it as a failure of its own, not as a source
/// that could not be reached.
/// </summary>
public sealed class MigrationSourceAddressRefusedException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="MigrationSourceAddressRefusedException"/> class.</summary>
    /// <param name="innerException">What refused the address; logged by the caller, never shown to a user. Null when the refusal was decided before any connection.</param>
    public MigrationSourceAddressRefusedException(Exception? innerException = null)
        : base("The migration source address is not allowed.", innerException)
    {
    }
}
