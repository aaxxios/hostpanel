namespace Innovayse.Hostpanel.Application.Partners.Commands.DeletePartner;

/// <summary>Command to permanently delete a partner by id.</summary>
/// <param name="Id">The identifier of the partner to delete.</param>
public sealed record DeletePartnerCommand(int Id);
