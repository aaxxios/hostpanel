using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Fluid;
using Fluid.Values;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

namespace Innovayse.Hostpanel.Application.Notifications.Rendering;

/// <summary>
/// Renders Liquid templates with the caller's typed model plus the storefront's brand.
/// </summary>
/// <remarks>
/// Every render sees <c>brand</c> (<c>primary</c>, <c>primary_dark</c>, <c>on_primary</c>,
/// <c>accent</c>, <c>logo_url</c>) and <c>site</c> (<c>name</c>) beside the model's own members,
/// which Liquid reads in lower snake case (<see cref="EmailTemplateModel"/>). A body is HTML, so
/// every value it prints is HTML-encoded: a ticket subject or a client name typed by a visitor
/// reaches the inbox as text, never as markup. A subject is a plain-text header and is not encoded.
/// </remarks>
public sealed class TemplateRenderer
{
    /// <summary>
    /// The options every render shares: model members are read by their lower snake case name,
    /// including those of nested records, which a registration-based strategy would leave blank,
    /// and an amount is printed with the decimals it carries (<see cref="CreateOptions"/>).
    /// </summary>
    private static readonly TemplateOptions Options = CreateOptions();

    /// <summary>
    /// The encoder every body render shares. It encodes markup characters but, unlike the default
    /// encoder, leaves letters of every script as they are, so Russian and Armenian text stays readable
    /// instead of becoming numeric entities.
    /// </summary>
    private static readonly HtmlEncoder BodyEncoder = HtmlEncoder.Create(UnicodeRanges.All);

    /// <summary>The Fluid parser instance.</summary>
    private readonly FluidParser _parser = new();

    /// <summary>Where the brand comes from.</summary>
    private readonly IEmailBrandingProvider _branding;

    /// <summary>Initializes the renderer.</summary>
    /// <param name="branding">Where the brand comes from.</param>
    public TemplateRenderer(IEmailBrandingProvider branding)
    {
        _branding = branding;
    }

    /// <summary>Renders an HTML template (an email body), HTML-encoding every printed value.</summary>
    /// <param name="template">The Liquid template string to render.</param>
    /// <param name="model">The typed data the template reads.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The rendered HTML.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the template contains a parse error.</exception>
    public Task<string> RenderHtmlAsync(string template, EmailTemplateModel model, CancellationToken ct) =>
        RenderAsync(template, model, BodyEncoder, ct);

    /// <summary>Renders a plain-text template (an email subject), printing values as they are.</summary>
    /// <param name="template">The Liquid template string to render.</param>
    /// <param name="model">The typed data the template reads.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The rendered text.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the template contains a parse error.</exception>
    public Task<string> RenderTextAsync(string template, EmailTemplateModel model, CancellationToken ct) =>
        RenderAsync(template, model, NullEncoder.Default, ct);

    /// <summary>Parses and renders a template with the model, the brand and the given encoder.</summary>
    /// <param name="template">The Liquid template string to render.</param>
    /// <param name="model">The typed data the template reads.</param>
    /// <param name="encoder">How each printed value is encoded.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The rendered output.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the template contains a parse error.</exception>
    private async Task<string> RenderAsync(string template, EmailTemplateModel model, TextEncoder encoder, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!_parser.TryParse(template, out var fluidTemplate, out var error))
        {
            throw new InvalidOperationException($"Template parse error: {error}");
        }

        var brand = await _branding.GetAsync(ct);

        var context = new TemplateContext(model, Options, allowModelMembers: true);
        context.SetValue("brand", brand.ToBrandModel());
        context.SetValue("site", brand.ToSiteModel());

        return await fluidTemplate.RenderAsync(context, encoder);
    }

    /// <summary>Builds the options every render shares.</summary>
    /// <remarks>
    /// Fluid prints a number without its trailing zeros, so an invoice total of 19.90 would read
    /// 19.9 in the mail. The only decimals a template model carries are amounts of money, so each one
    /// is handed to the template as the text of the amount with the decimals it was stored with.
    /// </remarks>
    /// <returns>The shared options.</returns>
    private static TemplateOptions CreateOptions()
    {
        var options = new TemplateOptions
        {
            MemberAccessStrategy = new UnsafeMemberAccessStrategy { MemberNameStrategy = MemberNameStrategies.SnakeCase },
        };
        options.ValueConverters.Add(value =>
            value is decimal amount ? new StringValue(amount.ToString(CultureInfo.InvariantCulture)) : null);
        return options;
    }
}
