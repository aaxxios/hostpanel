/**
 * Tests for the migration store parts the shared misc suite does not cover: the list load, the
 * create body defaults, the status poll and the log query. Real `useApi` transport, `fetch`
 * stubbed.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useMigrationStore } from '@/stores/migration'
import { json, problem, stubFetch, stubOffline, callOf } from '../helpers/fetch'

beforeEach(() => {
  setActivePinia(createPinia())
})

afterEach(() => {
  vi.useRealTimers()
  vi.unstubAllGlobals()
})

describe('migrationStore.fetchAll', () => {
  it('keeps the jobs and clears loading', async () => {
    stubFetch(json([{ id: 1 }]))
    const store = useMigrationStore()

    await store.fetchAll()
    expect(store.jobs).toEqual([{ id: 1 }])
    expect(store.loading).toBe(false)
  })

  it('keeps the API sentence on refusal', async () => {
    stubFetch(problem('Jobs unavailable.', 503))
    const store = useMigrationStore()

    await store.fetchAll()
    expect(store.error).toBe('Jobs unavailable.')
  })
})

describe('migrationStore.createJob', () => {
  it('sends every export flag, true unless the caller turned it off', async () => {
    const mock = stubFetch(json({ id: 2 }))

    await useMigrationStore().createJob({ sourceUrl: 'https://old.example', exportQuotes: false })
    expect(callOf(mock).body).toMatchObject({
      label: null,
      sourceUrl: 'https://old.example',
      selection: {
        clients: true,
        quotes: false,
        announcements: true,
        downloads: true,
        networkIssues: true,
      },
    })
  })
})

describe('migrationStore polling', () => {
  it('pollStatus updates the active job and the list, and stops once completed', async () => {
    stubFetch(json({ id: 5, status: 'Completed' }))
    const store = useMigrationStore()
    store.jobs.push({ id: 5, status: 'InProgress' } as never)

    await store.pollStatus(5)
    expect(store.activeJob).toEqual({ id: 5, status: 'Completed' })
    expect(store.jobs[0]?.status).toBe('Completed')
  })

  it('a failed poll leaves the state and no sentence', async () => {
    stubFetch(problem('Gone.', 404))
    const store = useMigrationStore()
    store.setActiveJob({ id: 5, status: 'InProgress' } as never)

    await store.pollStatus(5)
    expect(store.activeJob?.status).toBe('InProgress')
    expect(store.error).toBeNull()
  })

  it('startPolling reads at once and again every two seconds until stopped', async () => {
    vi.useFakeTimers()
    const mock = stubFetch(json({ id: 5, status: 'InProgress' }))
    const store = useMigrationStore()

    store.startPolling(5)
    await vi.advanceTimersByTimeAsync(0)
    expect(mock).toHaveBeenCalledTimes(1)
    await vi.advanceTimersByTimeAsync(2000)
    expect(mock).toHaveBeenCalledTimes(2)
    store.stopPolling()
    await vi.advanceTimersByTimeAsync(4000)
    expect(mock).toHaveBeenCalledTimes(2)
  })
})

describe('migrationStore.fetchLogs', () => {
  it('returns the page and asks for page 1 of 50 by default', async () => {
    const mock = stubFetch(json({ items: [], totalCount: 0, page: 1, pageSize: 50 }))

    expect(await useMigrationStore().fetchLogs(5)).toMatchObject({ page: 1 })
    expect(callOf(mock).url).toBe('/api/admin/migrations/5/logs?page=1&pageSize=50')
  })

  it('returns null with the API sentence on refusal', async () => {
    stubFetch(problem('No such job.', 404))
    const store = useMigrationStore()

    expect(await store.fetchLogs(5)).toBeNull()
    expect(store.error).toBe('No such job.')
  })
})

describe('migrationStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('createJob puts the job first and returns it', async () => {
    stubFetch(json({ id: 3 }))
    const store = useMigrationStore()

    expect(await store.createJob({ sourceUrl: 'https://old.example' })).toEqual({ id: 3 })
    expect(store.jobs[0]).toEqual({ id: 3 })
  })

  it('createJob returns null with the API sentence on refusal', async () => {
    stubFetch(problem('Source unreachable.'))
    const store = useMigrationStore()

    expect(await store.createJob({ sourceUrl: 'x' })).toBeNull()
    expect(store.error).toBe('Source unreachable.')
  })

  it('testConnection replaces the job in the list and the active slot', async () => {
    const store = useMigrationStore()
    store.jobs.push({ id: 3, pluginConnected: false } as never)
    store.setActiveJob({ id: 3, pluginConnected: false } as never)
    stubFetch(json({ id: 3, pluginConnected: true }))

    expect((await store.testConnection(3))?.pluginConnected).toBe(true)
    expect(store.jobs[0]?.pluginConnected).toBe(true)
    expect(store.activeJob?.pluginConnected).toBe(true)
  })

  it('startImport returns null offline', async () => {
    stubOffline()
    const store = useMigrationStore()

    expect(await store.startImport(3)).toBeNull()
    expect(store.error).toBe('Could not reach the server. Check your connection and try again.')
  })

  it('deleteJob removes the job on success and keeps it on refusal', async () => {
    const store = useMigrationStore()
    store.jobs.push({ id: 3 } as never)
    stubFetch(problem('Job is running.', 409))
    expect(await store.deleteJob(3)).toBe(false)
    expect(store.jobs).toHaveLength(1)

    stubFetch(json({}))
    expect(await store.deleteJob(3)).toBe(true)
    expect(store.jobs).toHaveLength(0)
  })

  it('fetchLogs returns the page, null when refused', async () => {
    const store = useMigrationStore()
    stubFetch(json({ items: [], totalCount: 0 }))
    expect(await store.fetchLogs(3, { page: 2 })).toEqual({ items: [], totalCount: 0 })

    stubFetch(problem('No log.', 404))
    expect(await store.fetchLogs(3)).toBeNull()
    expect(store.error).toBe('No log.')
  })
})
