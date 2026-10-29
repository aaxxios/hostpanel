using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Domains.Common;

namespace Innovayse.Hostpanel.Application.Domains.Extensions;

/// <summary>
/// Picks the error code a domain handler answers with when the registrar refused. Every handler
/// that calls a registrar goes through this one rule, so the caller hears the same reason
/// whichever operation was refused.
/// </summary>
public static class RegistrarResultExtensions
{
    /// <summary>
    /// The registrar's own reason when it names one (not configured, no such domain, no such
    /// operation, not active yet, no privacy subscription), otherwise the handler's operation code.
    /// A plain refusal (<see cref="HostpanelErrorCodes.REGISTRAR_ORDER_UPSTREAM_FAILED"/>) or no code
    /// at all says nothing more than the operation code does, so the operation code wins there.
    /// </summary>
    /// <param name="result">The registrar's failed answer.</param>
    /// <param name="operationCode">The handler's own <c>*_UPSTREAM_FAILED</c> code for this operation.</param>
    /// <returns>The error code to fail with.</returns>
    public static string ToFailureCode(this RegistrarResult result, string operationCode)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.ErrorCode is null || result.ErrorCode == HostpanelErrorCodes.REGISTRAR_ORDER_UPSTREAM_FAILED)
        {
            return operationCode;
        }

        return result.ErrorCode;
    }
}
