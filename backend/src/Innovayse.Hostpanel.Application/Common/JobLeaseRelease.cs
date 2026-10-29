namespace Innovayse.Hostpanel.Application.Common;

/// <summary>What an instance reports to <see cref="Interfaces.IJobLease.CompleteAsync"/> when a pass ends.</summary>
/// <param name="JobName">The job's name (its command's name).</param>
/// <param name="Owner">Who held the lease.</param>
/// <param name="Now">The current moment.</param>
/// <param name="Succeeded">Whether the pass finished without failing.</param>
/// <param name="Rest">How long a successful pass keeps the job closed to other instances.</param>
public sealed record JobLeaseRelease(string JobName, string Owner, DateTimeOffset Now, bool Succeeded, TimeSpan Rest);
