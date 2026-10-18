using System.Text.RegularExpressions;

namespace Innovayse.Hostpanel.Domain.Settings;

/// <summary>
/// A shape a free-text key must match, with the sentence a refusal shows.
/// </summary>
/// <param name="Regex">The pattern; the whole trimmed value must match.</param>
/// <param name="Expected">What a valid value looks like, for the operator.</param>
/// <param name="Canonical">Turns an accepted value into the spelling that is stored.</param>
public sealed record ValuePattern(Regex Regex, string Expected, Func<string, string> Canonical);
