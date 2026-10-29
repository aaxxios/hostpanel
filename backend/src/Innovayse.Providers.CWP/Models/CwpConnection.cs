namespace Innovayse.Providers.CWP.Models;

/// <summary>Where and how the CWP client reaches one CWP server.</summary>
/// <param name="BaseUrl">CWP server base URL including port, e.g. "https://cwp.example.com:2304".</param>
/// <param name="ApiKey">CWP API key for authentication.</param>
internal sealed record CwpConnection(string BaseUrl, string ApiKey);
