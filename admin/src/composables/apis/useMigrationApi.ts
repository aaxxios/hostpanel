/**
 * Endpoints behind the migration store: import jobs, their connection probe, start, retry and log.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type {
  CreateMigrationJobPayload,
  MigrationJob,
  MigrationLogEntry,
  MigrationLogQuery,
} from '@/types/migration'
import type { PaginatedList } from '@/types/paginatedlist'
import { ApiPath } from '@/lib/apiPaths'
import { HttpMethod } from '@/types/http'

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
  const listJobs = (): Promise<MigrationJob[]> => request<MigrationJob[]>(ApiPath.AdminMigrations)

  /**
   * Reads one job.
   *
   * @param id - Job id.
   * @returns The job with its progress.
   */
  const getJob = (id: number): Promise<MigrationJob> =>
    request<MigrationJob>(`${ApiPath.AdminMigrations}/${id}`)

  /**
   * Creates a job.
   *
   * @param payload - Source, label and the export flags.
   * @returns The created job.
   */
  const createJob = (payload: CreateMigrationJobPayload): Promise<MigrationJob> =>
    request<MigrationJob>(ApiPath.AdminMigrations, { method: HttpMethod.Post, body: JSON.stringify(payload) })

  /**
   * Deletes a job.
   *
   * @param id - Job id.
   * @returns Nothing; the API answers 204.
   */
  const deleteJob = (id: number): Promise<void> =>
    request<void>(`${ApiPath.AdminMigrations}/${id}`, { method: HttpMethod.Delete })

  /**
   * Probes the source plugin connection of a job.
   *
   * @param id - Job id.
   * @returns The job with its refreshed connection state.
   */
  const testJobConnection = (id: number): Promise<MigrationJob> =>
    request<MigrationJob>(`${ApiPath.AdminMigrations}/${id}/test-connection`, { method: HttpMethod.Post })

  /**
   * Starts the import of a job.
   *
   * @param id - Job id.
   * @returns The job in its new state.
   */
  const startJob = (id: number): Promise<MigrationJob> =>
    request<MigrationJob>(`${ApiPath.AdminMigrations}/${id}/start`, { method: HttpMethod.Post })

  /**
   * Queues a failed job's import again.
   *
   * @param id - Job id.
   * @returns The job in its new state.
   */
  const retryJob = (id: number): Promise<MigrationJob> =>
    request<MigrationJob>(`${ApiPath.AdminMigrations}/${id}/retry`, { method: HttpMethod.Post })

  /**
   * Reads one page of a job's import log.
   *
   * @param id - Job id.
   * @param query - Optional filters and the page to read.
   * @returns The log page.
   */
  const getJobLogs = (id: number, query: MigrationLogQuery): Promise<PaginatedList<MigrationLogEntry>> =>
    request<PaginatedList<MigrationLogEntry>>(
      `${ApiPath.AdminMigrations}/${id}/logs${buildQuery({
        action: query.action,
        entityType: query.entityType,
        page: query.page,
        pageSize: query.pageSize,
      })}`,
    )

  return { listJobs, getJob, createJob, deleteJob, testJobConnection, startJob, retryJob, getJobLogs }
}
