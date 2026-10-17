using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Slides.Queries.ListActiveSlides;

/// <summary>Validates <see cref="ListActiveSlidesQuery"/> before it reaches the handler.</summary>
/// <remarks>
/// <see cref="ListActiveSlidesQuery.Audience"/> is only ever <see langword="null"/> or a value the
/// controller already parsed from a known enum name — an unparsable audience string never reaches
/// here — so <c>IsInEnum</c> is defensive rather than load-bearing.
/// </remarks>
public sealed class ListActiveSlidesValidator : LocalizedValidator<ListActiveSlidesQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListActiveSlidesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Audience).IsInEnum().When(x => x.Audience is not null)
            .WithMessage(L["SlideTargetAudienceInvalid"]);
    }
}
