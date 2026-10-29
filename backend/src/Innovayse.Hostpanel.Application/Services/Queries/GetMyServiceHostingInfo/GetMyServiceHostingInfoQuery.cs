using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetMyServiceHostingInfo;

/// <summary>
/// Query for a signed-in client to read the live nameservers and certificate of the domain on one of
/// their own services.
/// </summary>
/// <remarks>
/// Carries a service id and no domain: the domain is read from the service row, so a caller cannot
/// point the probe at a name of their choosing.
/// </remarks>
/// <param name="ServiceId">Primary key of the service, which must belong to the caller.</param>
public sealed record GetMyServiceHostingInfoQuery(int ServiceId) : ICallerScopedServiceMessage;
