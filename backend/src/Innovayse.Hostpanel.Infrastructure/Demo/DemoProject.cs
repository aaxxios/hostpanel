namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>A demo portfolio project; the translated fields carry one entry per demo locale.</summary>
/// <param name="Slug">The project slug, also naming its preview file.</param>
/// <param name="Title">The title, per locale.</param>
/// <param name="Summary">The summary, per locale.</param>
internal sealed record DemoProject(string Slug, string[] Title, string[] Summary);
