namespace Innovayse.Hostpanel.Application.Partners.Queries.GetPartner;

/// <summary>Query to retrieve a single partner by id for the admin panel.</summary>
/// <param name="Id">The identifier of the partner to retrieve.</param>
public sealed record GetPartnerQuery(int Id);
