namespace Innovayse.Providers.CWP7.Models;

/// <summary>Where and how the CWP7 client reaches one CWP7 server.</summary>
/// <param name="BaseUrl">CWP7 API base URL including port, e.g. "https://cwp.example.com:2304".</param>
/// <param name="ApiKey">CWP7 API key for authentication.</param>
/// <param name="UserPanelUrl">
/// Base URL of the panel's user-facing sign-in page, e.g. "https://cwp.example.com:2083". It is a
/// different port from <paramref name="BaseUrl"/> and is only shown to the account owner.
/// </param>
internal sealed record Cwp7Connection(string BaseUrl, string ApiKey, string UserPanelUrl);
