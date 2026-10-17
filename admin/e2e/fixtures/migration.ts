/**
 * Fixtures for the migration screen: a freshly created job as `/api/admin/migrations` returns it.
 */
import { MIGRATION_ENTITY_KEYS, MigrationStatus } from '../../src/types/migration'
import type { MigrationJob } from '../../src/types/migration'

/** Every entity group, none yet imported. */
const progress = {} as MigrationJob['progress']

/** Clients and invoices selected, the rest left out. */
const selection = {} as MigrationJob['entitySelection']

for (const key of MIGRATION_ENTITY_KEYS) {
  progress[key] = { imported: 0, skipped: 0, total: 0, done: false }
  selection[key] = key === 'clients' || key === 'invoices'
}

/** A job waiting for the source plugin to connect. */
export const pendingJob: MigrationJob = {
  id: 5,
  key: 'secret-key-123',
  sourceUrl: 'https://old.example.test',
  status: MigrationStatus.Pending,
  label: 'Production',
  errorMessage: null,
  entitySelection: selection,
  progress,
  overallPercent: 0,
  pluginConnected: false,
  lastPingAt: null,
  createdAt: '2026-10-01T00:00:00Z',
  completedAt: null,
}
