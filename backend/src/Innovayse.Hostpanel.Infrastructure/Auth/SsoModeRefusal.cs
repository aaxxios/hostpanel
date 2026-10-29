namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// The one fault the sign-on mode stand-ins for the local account ports raise when they are asked to
/// write: those ports have no failure channel of their own for a write, and a write reaching them means a
/// local-only route got past <c>LocalModeOnlyFilter</c>, which is a fault, not a refusal a caller can act on.
/// </summary>
public static class SsoModeRefusal
{
    /// <summary>
    /// Builds the fault for one operation, naming it so the log says which flow tried to reach the local
    /// account store in a deployment that has none.
    /// </summary>
    /// <param name="operation">The port member that was called.</param>
    /// <returns>
    /// The exception to throw; the same type <c>IUserService.CreateAsync</c> documents for a refused account.
    /// </returns>
    public static InvalidOperationException LocalAccountsOnly(string operation) =>
        new($"{operation} needs the local account store, which a deployment whose people belong to the "
            + "sign-on provider does not have (Auth:Mode is not 'local').");
}
