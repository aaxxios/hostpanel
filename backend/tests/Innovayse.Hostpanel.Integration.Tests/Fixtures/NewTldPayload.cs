namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A unique extension and the body that creates it.
/// </summary>
/// <param name="Tld">The generated extension.</param>
/// <param name="Body">The request body that creates the extension.</param>
public sealed record NewTldPayload(string Tld, object Body);
