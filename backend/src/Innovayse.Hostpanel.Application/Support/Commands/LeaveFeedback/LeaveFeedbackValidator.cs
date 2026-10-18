using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.LeaveFeedback;

/// <summary>Validator for <see cref="LeaveFeedbackCommand"/>.</summary>
public sealed class LeaveFeedbackValidator : LocalizedValidator<LeaveFeedbackCommand>
{
    /// <summary>Lowest rating accepted.</summary>
    private const int MinRating = 1;

    /// <summary>Highest rating accepted.</summary>
    private const int MaxRating = 10;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public LeaveFeedbackValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage(L["TicketIdInvalid"]);

        RuleFor(x => x.Rating)
            .InclusiveBetween(MinRating, MaxRating)
            .WithMessage(L["TicketRatingInvalid"]);

        RuleFor(x => x.LeftBy)
            .NotEmpty()
            .WithMessage(L["TicketFeedbackLeftByRequired"]);
    }
}
