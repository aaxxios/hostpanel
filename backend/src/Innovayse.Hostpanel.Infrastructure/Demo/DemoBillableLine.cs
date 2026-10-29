namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>A one-off or recurring charge a demo billable item is made of.</summary>
/// <param name="Description">The charge description.</param>
/// <param name="Amount">The dollar amount.</param>
internal sealed record DemoBillableLine(string Description, decimal Amount);
