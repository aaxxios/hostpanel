/**
 * Fixtures of the migration screens' tests: a complete job as the admin API returns it.
 */
import { MIGRATION_ENTITY_KEYS, MigrationStatus } from '@/types/migration'
import type { MigrationJob } from '@/types/migration'

/**
 * Builds a migration job with every entity group selected and untouched.
 *
 * @param overrides - Fields to change on the default job.
 * @returns The job.
 */
export const migrationJob = (overrides: Partial<MigrationJob> = {}): MigrationJob => {
  const selection = {} as MigrationJob['entitySelection']
  const progress = {} as MigrationJob['progress']
  for (const key of MIGRATION_ENTITY_KEYS) {
    selection[key] = key === 'clients' || key === 'invoices'
    progress[key] = { imported: 0, skipped: 0, failed: 0, total: 0, done: false }
  }
  return {
    id: 5,
    key: 'secret-key-123',
    sourceUrl: 'https://old.example.com',
    status: MigrationStatus.Pending,
    label: 'Production',
    failure: null,
    entitySelection: selection,
    progress: progress,
    overallPercent: 0,
    pluginConnected: false,
    lastPingAt: null,
    createdAt: '2026-10-01T00:00:00Z',
    completedAt: null,
    ...overrides,
  }
}
