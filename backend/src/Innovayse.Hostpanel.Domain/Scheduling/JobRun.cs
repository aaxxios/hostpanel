namespace Innovayse.Hostpanel.Domain.Scheduling;

/// <summary>
/// The run record of one recurring job, shared by every instance of the application: who holds the
/// job right now, until when, and when it last started and last succeeded. One row per job name.
/// Created via <see cref="Create"/>: no public constructor.
/// </summary>
/// <remarks>
/// The lease is what keeps two instances from running the same job at once: an instance takes it
/// before a pass and gives it back after. A holder that crashes never gives it back, so the lease
/// carries an expiry and another instance takes it over once that moment has passed.
/// </remarks>
public sealed class JobRun
{
    /// <summary>Gets the job's name (the name of its command), the row's key.</summary>
    public string JobName { get; private set; } = string.Empty;

    /// <summary>Gets who holds the lease, or <see langword="null"/> when nobody does.</summary>
    public string? LeaseOwner { get; private set; }

    /// <summary>Gets the moment the current lease (or the rest after a successful pass) ends.</summary>
    public DateTimeOffset? LeaseUntil { get; private set; }

    /// <summary>Gets when the last pass was started, or <see langword="null"/> when none has.</summary>
    public DateTimeOffset? LastStartedAt { get; private set; }

    /// <summary>Gets when the last successful pass finished, or <see langword="null"/> when none has.</summary>
    public DateTimeOffset? LastSucceededAt { get; private set; }

    /// <summary>EF Core parameterless constructor: do not call directly.</summary>
    private JobRun() { }

    /// <summary>Creates the record of a job that has never run.</summary>
    /// <param name="jobName">The job's name.</param>
    /// <returns>A new, unpersisted <see cref="JobRun"/> that nobody holds.</returns>
    public static JobRun Create(string jobName)
    {
        return new JobRun { JobName = jobName };
    }

    /// <summary>Takes the lease when nobody holds it or the previous lease has ended.</summary>
    /// <param name="owner">Who takes the lease; the same value is needed to give it back.</param>
    /// <param name="now">The current moment.</param>
    /// <param name="duration">How long the lease lasts if its holder never gives it back.</param>
    /// <returns><see langword="true"/> when the caller now holds the lease; <see langword="false"/> when another holder has it.</returns>
    public bool TryAcquire(string owner, DateTimeOffset now, TimeSpan duration)
    {
        if (LeaseUntil is not null && LeaseUntil > now)
        {
            return false;
        }

        LeaseOwner = owner;
        LeaseUntil = now + duration;
        LastStartedAt = now;
        return true;
    }

    /// <summary>
    /// Gives the lease back after a pass. A successful pass is recorded and keeps the job closed to
    /// other instances for <paramref name="rest"/> (not at all when it is zero), so an instance whose
    /// own timer fires a moment later does not repeat the pass that has just finished; a failed pass
    /// frees the job at once.
    /// </summary>
    /// <param name="owner">Who holds the lease.</param>
    /// <param name="now">The current moment.</param>
    /// <param name="succeeded">Whether the pass finished without failing.</param>
    /// <param name="rest">How long a successful pass keeps the job closed to other instances.</param>
    /// <returns><see langword="false"/> when the caller no longer holds the lease (it expired and was taken over); nothing changes then.</returns>
    public bool Complete(string owner, DateTimeOffset now, bool succeeded, TimeSpan rest)
    {
        if (LeaseOwner != owner)
        {
            return false;
        }

        LeaseOwner = null;
        if (succeeded)
        {
            LastSucceededAt = now;
            LeaseUntil = rest > TimeSpan.Zero ? now + rest : null;
        }
        else
        {
            LeaseUntil = null;
        }

        return true;
    }
}
