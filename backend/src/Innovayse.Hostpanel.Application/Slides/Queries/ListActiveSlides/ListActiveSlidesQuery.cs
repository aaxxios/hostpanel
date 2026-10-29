namespace Innovayse.Hostpanel.Application.Slides.Queries.ListActiveSlides;

/// <summary>Query to retrieve the active slides for public display; the translation follows the request language.</summary>
/// <remarks>
/// Carries no input and so has no validator. In particular it carries no audience: a caller-chosen
/// audience would let an anonymous visitor ask for the slides meant for signed-in clients. The
/// handler decides the audience from <c>ICurrentUserService</c>.
/// </remarks>
public sealed record ListActiveSlidesQuery;
