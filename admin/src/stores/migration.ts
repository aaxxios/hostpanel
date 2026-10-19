/**
 * Pinia store for migration jobs: the job list, the job being watched, status polling, the
 * connection probe, start and the import log.
 *
 * The only caller of `useMigrationApi`; a refusal is kept as the API's own sentence in `error`,
 * never thrown.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useMigrationApi } from '@/composables/apis/useMigrationApi'
import { runAction } from '@/lib/runAction'
import { MigrationStatus } from '@/types/migration'
import type {
  CreateMigrationJobOptions,
  MigrationJob,
  MigrationLogPage,
  MigrationLogQuery,
} from '@/types/migration'
import { POLL_INTERVAL_MS } from '@/lib/limits'

/** Holds migration jobs and drives their lifecycle from the admin panel. */
export const useMigrationStore = defineStore('migration', () => {
  const api = useMigrationApi()

  /** All migration jobs, newest first. */
  const jobs = ref<MigrationJob[]>([])

  /** The job being watched, null when none. */
  const activeJob = ref<MigrationJob | null>(null)

  /** True while the job list is loading. */
  const loading = ref(false)

  /** Sentence for the last failed request, null when none. */
  const error = ref<string | null>(null)

  /** Handle of the running status poll, null when not polling. */
  let pollTimer: ReturnType<typeof setInterval> | null = null

  /**
   * Fetches all migration jobs.
   *
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const fetchAll = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, api.listJobs)
    loading.value = false
    if (result.ok) {
      jobs.value = result.value
    }
  }

  /**
   * Stops the status poll, if one is running.
   *
   * @returns Nothing.
   */
  const stopPolling = (): void => {
    if (pollTimer) {
      clearInterval(pollTimer)
      pollTimer = null
    }
  }

  /**
   * Deletes a migration job and drops it from the list.
   *
   * @param id - Migration job id.
   * @returns True when deleted; false when refused (see `error`).
   */
  const deleteJob = async (id: number): Promise<boolean> => {
    const result = await runAction(error, () => api.deleteJob(id))
    if (result.ok) {
      jobs.value = jobs.value.filter(j => j.id !== id)
      if (activeJob.value?.id === id) {
        stopPolling()
        activeJob.value = null
      }
    }
    return result.ok
  }

  /**
   * Creates a migration job and puts it first in the list.
   *
   * @param opts - Label, source URL and the entity groups to export (each defaults to true).
   * @returns The created job, or null when refused (see `error`).
   */
  const createJob = async (opts: CreateMigrationJobOptions): Promise<MigrationJob | null> => {
    const created = await runAction(error, () => api.createJob({
      label: opts.label ?? null,
      sourceUrl: opts.sourceUrl,
      selection: {
        clients: opts.exportClients ?? true,
        invoices: opts.exportInvoices ?? true,
        services: opts.exportServices ?? true,
        domains: opts.exportDomains ?? true,
        tickets: opts.exportTickets ?? true,
        products: opts.exportProducts ?? true,
        orders: opts.exportOrders ?? true,
        transactions: opts.exportTransactions ?? true,
        quotes: opts.exportQuotes ?? true,
        knowledgebase: opts.exportKnowledgebase ?? true,
        contacts: opts.exportContacts ?? true,
        ticketReplies: opts.exportTicketReplies ?? true,
        announcements: opts.exportAnnouncements ?? true,
        downloads: opts.exportDownloads ?? true,
        networkIssues: opts.exportNetworkIssues ?? true,
      },
    }))
    if (!created.ok) {
      return null
    }
    jobs.value.unshift(created.value)
    return created.value
  }

  /**
   * Replaces a job in the list (and the active slot) with a fresh copy from the API.
   *
   * @param job - The job as the API returned it.
   * @returns Nothing.
   */
  const replaceJob = (job: MigrationJob): void => {
    const idx = jobs.value.findIndex(j => j.id === job.id)
    if (idx >= 0) {
      jobs.value[idx] = job
    }
    if (activeJob.value?.id === job.id) {
      activeJob.value = job
    }
  }

  /**
   * Probes the source plugin connection of a job.
   *
   * @param id - Migration job id.
   * @returns The updated job, or null when refused (see `error`).
   */
  const testConnection = async (id: number): Promise<MigrationJob | null> => {
    const result = await runAction(error, () => api.testJobConnection(id))
    if (!result.ok) {
      return null
    }
    replaceJob(result.value)
    return result.value
  }

  /**
   * Starts the import of a job.
   *
   * @param id - Migration job id.
   * @returns The updated job, or null when refused (see `error`).
   */
  const startImport = async (id: number): Promise<MigrationJob | null> => {
    const result = await runAction(error, () => api.startJob(id))
    if (!result.ok) {
      return null
    }
    replaceJob(result.value)
    return result.value
  }

  /**
   * Re-reads one job, keeps it as the active job and stops polling once it is finished.
   *
   * A failed read ends the poll without surfacing a sentence: the next list load reports it.
   *
   * @param jobId - Migration job id.
   * @returns Resolves when the read is over.
   */
  const pollStatus = async (jobId: number): Promise<void> => {
    /** Local sink: a failed poll tick only ends the poll. */
    const ignored = ref<string | null>(null)
    const result = await runAction(ignored, () => api.getJob(jobId))
    if (!result.ok) {
      stopPolling()
      return
    }
    activeJob.value = result.value
    const idx = jobs.value.findIndex(j => j.id === jobId)
    if (idx >= 0) {
      jobs.value[idx] = result.value
    }
    if (result.value.status === MigrationStatus.Completed || result.value.status === MigrationStatus.Failed) {
      stopPolling()
    }
  }

  /**
   * Starts polling a job's status, replacing any poll already running.
   *
   * @param jobId - Migration job id.
   * @returns Nothing.
   */
  const startPolling = (jobId: number): void => {
    stopPolling()
    void pollStatus(jobId)
    pollTimer = setInterval(() => void pollStatus(jobId), POLL_INTERVAL_MS)
  }

  /**
   * Sets the job being watched.
   *
   * @param job - The job to watch.
   * @returns Nothing.
   */
  const setActiveJob = (job: MigrationJob): void => {
    activeJob.value = job
  }

  /**
   * Loads one page of a job's import log.
   *
   * @param jobId - Migration job id.
   * @param opts - Optional action/entity filters and paging.
   * @returns The log page, or null when refused (see `error`).
   */
  const fetchLogs = async (jobId: number, opts: MigrationLogQuery = {}): Promise<MigrationLogPage | null> => {
    const result = await runAction(error, () => api.getJobLogs(jobId, opts))
    return result.ok ? result.value : null
  }

  return { jobs, activeJob, loading, error, fetchAll, createJob, deleteJob, pollStatus, startPolling, stopPolling, setActiveJob, testConnection, startImport, fetchLogs }
})
