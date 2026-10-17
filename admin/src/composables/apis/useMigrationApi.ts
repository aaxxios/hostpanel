/**
 * Endpoints behind the migration store: import jobs, their connection probe, start and log.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type {
  CreateMigrationJobPayload,
  MigrationJob,
  MigrationLogPage,
  MigrationLogQuery,
} from '@/types/migration'

/**
 * Endpoint functions for `/admin/migrations`.
 *
 * @returns One typed function per endpoint.
 */
export const useMigrationApi = () => {
  const { request } = useApi()

  /**
   * Lists the migration jobs.
   *
   * @returns Every job.
   */
  const listJobs = (): Promise<MigrationJob[]> => request<MigrationJob[]>('/admin/migrations')

  /**
   * Reads one job.
   *
   * @param id - Job id.
   * @returns The job with its progress.
   */
  const getJob = (id: number): Promise<MigrationJob> =>
    request<MigrationJob>(`/admin/migrations/${id}`)

  /**
   * Creates a job.
   *
   * @param payload - Source, label and the export flags.
   * @returns The created job.
   */
  const createJob = (payload: CreateMigrationJobPayload): Promise<MigrationJob> =>
    request<MigrationJob>('/admin/migrations', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Deletes a job.
   *
   * @param id - Job id.
   * @returns Nothing; the API answers 204.
   */
  const deleteJob = (id: number): Promise<void> =>
    request<void>(`/admin/migrations/${id}`, { method: 'DELETE' })

  /**
   * Probes the source plugin connection of a job.
   *
   * @param id - Job id.
   * @returns The job with its refreshed connection state.
   */
  const testJobConnection = (id: number): Promise<MigrationJob> =>
    request<MigrationJob>(`/admin/migrations/${id}/test-connection`, { method: 'POST' })

  /**
   * Starts the import of a job.
   *
   * @param id - Job id.
   * @returns The job in its new state.
   */
  const startJob = (id: number): Promise<MigrationJob> =>
    request<MigrationJob>(`/admin/migrations/${id}/start`, { method: 'POST' })

  /**
   * Reads one page of a job's import log.
   *
   * @param id - Job id.
   * @param query - Optional filters and paging; page 1 of 50 by default.
   * @returns The log page.
   */
  const getJobLogs = (id: number, query: MigrationLogQuery = {}): Promise<MigrationLogPage> =>
    request<MigrationLogPage>(
      `/admin/migrations/${id}/logs${buildQuery({
        action: query.action,
        entityType: query.entityType,
        page: query.page ?? 1,
        pageSize: query.pageSize ?? 50,
      })}`,
    )

  return { listJobs, getJob, createJob, deleteJob, testJobConnection, startJob, getJobLogs }
}
