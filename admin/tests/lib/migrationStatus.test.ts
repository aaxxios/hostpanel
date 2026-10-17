/**
 * Unit tests for the migration presentation helpers: every job state and log outcome has a tone
 * and a caption key, and only Pending and InProgress jobs count as active.
 */
import { describe, expect, it } from 'vitest'
import { isJobActive, migrationActionLabelKey, migrationStatusLabelKey } from '@/lib/migrationStatus'
import { migrationActionTone, migrationStatusTone } from '@/lib/tones'
import { MigrationLogAction, MigrationStatus } from '@/types/migration'

describe('migrationStatus', () => {
  it('gives every job state a tone and a caption key', () => {
    for (const status of Object.values(MigrationStatus)) {
      expect(migrationStatusTone[status]).toBeTruthy()
      expect(migrationStatusLabelKey[status]).toMatch(/^migration\.status\./)
    }
  })

  it('gives every log outcome a tone and a caption key', () => {
    for (const action of Object.values(MigrationLogAction)) {
      expect(migrationActionTone[action]).toBeTruthy()
      expect(migrationActionLabelKey[action]).toMatch(/^migration\./)
    }
  })

  it('colours failure as danger and success as success', () => {
    expect(migrationStatusTone[MigrationStatus.Failed]).toBe('danger')
    expect(migrationStatusTone[MigrationStatus.Completed]).toBe('success')
    expect(migrationActionTone[MigrationLogAction.Skipped]).toBe('warning')
  })

  it('treats only pending and running jobs as active', () => {
    expect(isJobActive(MigrationStatus.Pending)).toBe(true)
    expect(isJobActive(MigrationStatus.InProgress)).toBe(true)
    expect(isJobActive(MigrationStatus.Completed)).toBe(false)
    expect(isJobActive(MigrationStatus.Failed)).toBe(false)
  })
})
