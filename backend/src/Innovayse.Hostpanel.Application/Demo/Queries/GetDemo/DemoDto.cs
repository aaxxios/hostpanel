namespace Innovayse.Hostpanel.Application.Demo.Queries.GetDemo;

/// <summary>What the public demo endpoint tells a visitor.</summary>
/// <param name="Enabled">Whether this deployment is a demo.</param>
/// <param name="NextResetAt">When the data is next wiped and reseeded; <see langword="null"/> when not a demo or not yet scheduled.</param>
/// <param name="Accounts">The published sign-ins; empty when not a demo.</param>
public sealed record DemoDto(bool Enabled, DateTimeOffset? NextResetAt, IReadOnlyList<DemoAccountDto> Accounts);
