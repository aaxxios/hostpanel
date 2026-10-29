namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>A demo server location; the translated fields carry one entry per demo locale.</summary>
/// <param name="Country">The country code.</param>
/// <param name="Name">The location name, per locale.</param>
/// <param name="City">The city, per locale.</param>
/// <param name="Note">The note, per locale.</param>
internal sealed record DemoServerLocation(string Country, string[] Name, string[] City, string[] Note);
