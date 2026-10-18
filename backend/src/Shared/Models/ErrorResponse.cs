namespace Innovayse.Hostpanel.Shared.Models;

/// <summary>
///     Standard error response structure matching api.md specification
/// </summary>
/// <param name="Type">Error type code (e.g., VALIDATION_ERROR, NOT_FOUND)</param>
/// <param name="Title">Human-readable error title</param>
/// <param name="Status">HTTP status code</param>
/// <param name="Detail">Detailed error message</param>
/// <param name="Errors">Dictionary of field names to error messages (for validation errors)</param>
public sealed record ErrorResponse(
    string Type,
    string Title,
    int Status,
    string Detail,
    Dictionary<string, List<string>>? Errors = null);
