namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>The answer to ending a local browser session.</summary>
/// <param name="Success">Always <see langword="true"/>: clearing a cookie that was not there is not a failure.</param>
public sealed record SignedOutDto(bool Success);
