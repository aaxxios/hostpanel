namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>The registrar a domain is registered with and the identifiers that registrar holds for it.</summary>
/// <param name="Module">Name of the registrar module; null if not set.</param>
/// <param name="Reference">Reference ID from the registrar's system; null if not yet registered.</param>
/// <param name="EppCode">Authorization code for transfer; null if not available.</param>
public sealed record DomainRegistrarDto(string? Module, string? Reference, string? EppCode);
