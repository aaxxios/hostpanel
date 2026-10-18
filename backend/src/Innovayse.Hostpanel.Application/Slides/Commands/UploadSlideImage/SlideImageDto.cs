namespace Innovayse.Hostpanel.Application.Slides.Commands.UploadSlideImage;

/// <summary>The URL a freshly uploaded slide image is served from.</summary>
/// <param name="Url">Root-relative URL, e.g. <c>/uploads/slides/&lt;generated&gt;.png</c>.</param>
public sealed record SlideImageDto(string Url);
