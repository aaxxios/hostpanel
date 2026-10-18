using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Microsoft.Extensions.Localization;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>
/// A validator deriving from <see cref="LocalizedValidator{T}"/> takes its field messages from the
/// localizer it was given, by key, so the sentence is the resource's and never a literal.
/// </summary>
/// <remarks>No mock of our own code: <see cref="EchoLocalizer"/> stands in
/// for the framework's <see cref="IStringLocalizer"/>, not for anything this product owns.</remarks>
public sealed class LocalizedValidatorTests
{
    /// <summary>A message with one field to validate.</summary>
    /// <param name="Name">The field the probe validator requires.</param>
    private sealed record Probe(string Name);

    /// <summary>A validator declaring one rule whose message is looked up by key.</summary>
    private sealed class ProbeValidator : LocalizedValidator<Probe>
    {
        /// <summary>Declares the rule.</summary>
        /// <param name="localizer">The sentences the rule's message is looked up in.</param>
        public ProbeValidator(IStringLocalizer localizer)
            : base(localizer)
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage(L["NameRequired"]);
        }
    }

    /// <summary>
    /// A localizer that answers every key with a marked copy of the key, so the test sees exactly
    /// which key a rule asked for. A stand-in for the framework's resource localizer, not our code.
    /// </summary>
    private sealed class EchoLocalizer : IStringLocalizer
    {
        /// <inheritdoc />
        public LocalizedString this[string name] => new(name, "text:" + name);

        /// <inheritdoc />
        public LocalizedString this[string name, params object[] arguments] => this[name];

        /// <inheritdoc />
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return [];
        }
    }

    /// <summary>A refused field carries the sentence the localizer holds under the rule's key.</summary>
    [Fact]
    public void Validate_RefusedField_UsesTheLocalizedSentence()
    {
        var validator = new ProbeValidator(new EchoLocalizer());

        var result = validator.Validate(new Probe(string.Empty));

        var error = Assert.Single(result.Errors);
        Assert.Equal("text:NameRequired", error.ErrorMessage);
    }

    /// <summary>A valid message passes with no errors.</summary>
    [Fact]
    public void Validate_ValidMessage_Passes()
    {
        var validator = new ProbeValidator(new EchoLocalizer());

        var result = validator.Validate(new Probe("x"));

        Assert.True(result.IsValid);
    }
}
