using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="IExchangeRateSource"/> that quotes what a test told it to and records what it was
/// asked for. A fake port standing in for the upstream rate service, which a test must not call.
/// </summary>
/// <param name="quotes">The rates to answer with, keyed by currency code.</param>
public sealed class FakeExchangeRateSource(IReadOnlyDictionary<string, decimal> quotes) : IExchangeRateSource
{
    /// <summary>The codes the last call asked to be quoted.</summary>
    public IReadOnlyList<string> RequestedCodes { get; private set; } = [];

    /// <summary>The base currency the last call asked the rates against.</summary>
    public string? RequestedBase { get; private set; }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, decimal>> RatesToAsync(
        string baseCode, IEnumerable<string> codes, CancellationToken ct)
    {
        RequestedBase = baseCode;
        RequestedCodes = [.. codes];

        IReadOnlyDictionary<string, decimal> answer = RequestedCodes
            .Where(quotes.ContainsKey)
            .ToDictionary(code => code, code => quotes[code]);

        return Task.FromResult(answer);
    }
}
