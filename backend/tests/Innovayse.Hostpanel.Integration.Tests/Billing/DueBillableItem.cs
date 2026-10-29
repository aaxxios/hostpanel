using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// A billable item that is due, and the client it belongs to.
/// </summary>
/// <param name="ClientId">The client's id.</param>
/// <param name="Item">The due item.</param>
public sealed record DueBillableItem(int ClientId, BillableItem Item);
