using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Common.Validators;

/// <summary>
///     Base validator with localization support.
///     All validators MUST inherit from this class for consistent localized error messages.
/// </summary>
/// <typeparam name="T">The command or query being validated.</typeparam>
/// <remarks>
/// <para>
/// **Every** validator derives from this, including the ones that declare no rules today. An
/// injected localizer a rule-less validator never reads is a small cost; the alternative costs
/// more, because then the base class tells a reader nothing and every validator has to be opened
/// to learn which one it uses. A validator that gains its first rule also gains its message
/// source with no edit to its signature.
/// </para>
/// <para>
/// Validators take a localizer and handlers do not, because of where each sentence is produced.
/// A handler's refusal is resolved at the API boundary — the error code is the resource key, and
/// <c>BaseApiController</c> looks it up, so a handler never needs a localizer. A validator's
/// message has no code to be resolved by: it is attached to a field, several can be returned at
/// once, and they are grouped into the <c>errors</c> dictionary before any controller sees them.
/// The validator is the only place that can localize its own sentence, and this base class is how
/// it does it.
/// </para>
/// </remarks>
public abstract class LocalizedValidator<T> : AbstractValidator<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LocalizedValidator{T}"/> class.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages. A concrete validator
    /// injects the typed <c>IStringLocalizer&lt;ApplicationValidationMessages&gt;</c> and passes it up.</param>
    protected LocalizedValidator(IStringLocalizer localizer)
    {
        L = localizer;
    }

    /// <summary>Gets the localizer a sentence is looked up by key through: <c>L["RequestIdRequired"]</c>.</summary>
    /// <remarks>Keys live in <c>Application/Resources/ApplicationValidationMessages.resx</c> and its
    /// <c>.ru</c>/<c>.hy</c> siblings, named for the rule rather than for the sentence. A value
    /// that interpolates takes <c>{0}</c> placeholders and is called as
    /// <c>L["Key", argument]</c> — never rebuilt with string interpolation, which a resource
    /// cannot do.</remarks>
    protected IStringLocalizer L { get; }
}
