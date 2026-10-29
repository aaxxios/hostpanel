namespace Innovayse.Hostpanel.Application.Common;

/// <summary>
/// Which page of people to read from the identity provider, and how to filter them.
/// </summary>
/// <param name="Search">
/// Text matched against the address and the name, or <see langword="null"/> for everyone.
/// </param>
/// <param name="Page">The 1-based page number.</param>
/// <param name="PageSize">How many people one page holds.</param>
public sealed record IdentityAccountListQuery(string? Search, int Page, int PageSize);
