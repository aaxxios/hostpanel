namespace Innovayse.Hostpanel.Infrastructure.Integrations.Migration;

/// <summary>The body of a source call that takes no arguments: a ping or a totals request.</summary>
/// <param name="Key">The job's shared key the source checks before answering.</param>
/// <param name="Action">The operation to run, in the source's own vocabulary.</param>
internal sealed record MigrationSourceActionWire(string Key, string Action);
