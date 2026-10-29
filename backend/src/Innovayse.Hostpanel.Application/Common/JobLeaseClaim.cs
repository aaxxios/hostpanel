namespace Innovayse.Hostpanel.Application.Common;

/// <summary>What an instance asks <see cref="Interfaces.IJobLease.TryAcquireAsync"/> for: the lease of one job.</summary>
/// <param name="JobName">The job's name (its command's name).</param>
/// <param name="Owner">Who asks; the same value is needed to give the lease back.</param>
/// <param name="Now">The current moment.</param>
/// <param name="Duration">How long the lease lasts if its holder never gives it back.</param>
public sealed record JobLeaseClaim(string JobName, string Owner, DateTimeOffset Now, TimeSpan Duration);
