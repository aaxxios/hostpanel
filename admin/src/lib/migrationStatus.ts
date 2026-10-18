/**
 * Presentation helpers shared by the migration screens: which caption a job state or an
 * import-log outcome gets, and whether a job can still change. Kept in one place so the history
 * list, the progress card and the report table cannot disagree. Badge tones live in `tones.ts`.
 */
import { MigrationLogAction, MigrationStatus } from '@/types/migration'
import type { MigrationJob, MigrationLogEntry } from '@/types/migration'

/** Locale key of the caption per job state. */
export const migrationStatusLabelKey: Record<MigrationJob['status'], string> = {
  [MigrationStatus.Pending]: 'migration.status.pending',
  [MigrationStatus.InProgress]: 'migration.status.inProgress',
  [MigrationStatus.Completed]: 'migration.status.completed',
  [MigrationStatus.Failed]: 'migration.status.failed',
}

/** Locale key of the caption per import-log outcome. */
export const migrationActionLabelKey: Record<MigrationLogEntry['action'], string> = {
  [MigrationLogAction.Imported]: 'migration.migrationReportTable.imported',
  [MigrationLogAction.Skipped]: 'migration.migrationProgressCard.skipped',
  [MigrationLogAction.Failed]: 'migration.migrationProgressCard.failed',
}

/**
 * Tells whether a job can still change on the server (so the screen polls it).
 *
 * @param status - The job's state.
 * @returns True for Pending and InProgress.
 */
export const isJobActive = (status: MigrationJob['status']): boolean =>
  status === MigrationStatus.Pending || status === MigrationStatus.InProgress
