using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>A client with two billable items seeded straight into the database.</summary>
/// <param name="ClientId">The client's primary key.</param>
/// <param name="First">The first seeded item.</param>
/// <param name="Second">The second seeded item.</param>
internal sealed record SeededBillableItems(int ClientId, BillableItem First, BillableItem Second);
