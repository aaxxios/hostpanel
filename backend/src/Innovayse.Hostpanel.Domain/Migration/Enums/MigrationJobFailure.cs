namespace Innovayse.Hostpanel.Domain.Migration.Enums;

/// <summary>Why a migration job stopped as <see cref="MigrationJobStatus.Failed"/>.</summary>
/// <remarks>
/// The stored form of each value is its name: never rename a member, rows already hold it.
/// </remarks>
public enum MigrationJobFailure
{
    /// <summary>The job failed for a reason this list does not name, including rows written before the reason was recorded as a value.</summary>
    Unknown = 0,

    /// <summary>The source plugin could not be reached: no connection, or no answer in time.</summary>
    SourceUnreachable = 1,

    /// <summary>The source plugin answered, but with a non-success status, for example a wrong key.</summary>
    SourceRefused = 2,

    /// <summary>The records were fetched but could not be read or saved here.</summary>
    ImportFailed = 3,

    /// <summary>The pull was stopped before it finished, for example by a shutdown.</summary>
    Cancelled = 4,

    /// <summary>The process running the pull stopped before the pull finished, so the job was left queued or in progress with no worker behind it.</summary>
    Interrupted = 5,

    /// <summary>The job's source address is not one this server may call: a scheme that is not allowed, or a host that resolves only to internal addresses.</summary>
    SourceAddressNotAllowed = 6,
}
