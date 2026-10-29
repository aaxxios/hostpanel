namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>A service a demo quote proposes.</summary>
/// <param name="Description">The line description.</param>
/// <param name="Price">The dollar price of one unit.</param>
internal sealed record DemoQuoteLine(string Description, decimal Price);
