namespace Innovayse.Hostpanel.Domain.Migration.Enums;
/// <summary>Lifecycle status of a migration job.</summary>
public enum MigrationJobStatus
{
    /// <summary>Job created, waiting for the migration plugin to connect.</summary>
    Pending = 0,

    /// <summary>Migration plugin is actively sending data.</summary>
    InProgress = 1,

    /// <summary>All data imported successfully.</summary>
    Completed = 2,

    /// <summary>Migration stopped due to an error.</summary>
    Failed = 3,
}
