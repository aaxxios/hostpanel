/**
 * Tests for the migration API composable: paths, verbs, bodies and the log query through the
 * real `useApi` transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useMigrationApi } from '@/composables/apis/useMigrationApi'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'
import { MigrationEntityType, MigrationLogAction } from '@/types/migration'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useMigrationApi', () => {
  it('listJobs and getJob read their endpoints', async () => {
    const mock = stubFetch(json([]))
    const api = useMigrationApi()

    await api.listJobs()
    await api.getJob(3)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/migrations', method: 'GET' })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/migrations/3', method: 'GET' })
  })

  it('createJob POSTs the payload and returns the job', async () => {
    const mock = stubFetch(json({ id: 3 }))

    expect(await useMigrationApi().createJob({ sourceUrl: 'https://old.example' } as never)).toEqual({ id: 3 })
    expect(callOf(mock)).toMatchObject({
      url: '/api/admin/migrations',
      method: 'POST',
      body: { sourceUrl: 'https://old.example' },
    })
  })

  it('deleteJob, testJobConnection, startJob and retryJob use their verbs and paths', async () => {
    const mock = stubFetch(json({ id: 3 }))
    const api = useMigrationApi()

    await api.deleteJob(3)
    await api.testJobConnection(3)
    await api.startJob(3)
    await api.retryJob(3)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/migrations/3', method: 'DELETE' })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/migrations/3/test-connection', method: 'POST' })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/admin/migrations/3/start', method: 'POST' })
    expect(callOf(mock, 3)).toMatchObject({ url: '/api/admin/migrations/3/retry', method: 'POST' })
  })

  it('getJobLogs sends the page and drops empty filters', async () => {
    const mock = stubFetch(json({ items: [] }))

    await useMigrationApi().getJobLogs(3, { page: 1, pageSize: 50 })
    expect(callOf(mock).url).toBe('/api/admin/migrations/3/logs?page=1&pageSize=50')
  })

  it('getJobLogs passes the filters and paging', async () => {
    const mock = stubFetch(json({ items: [] }))

    await useMigrationApi().getJobLogs(3, { action: MigrationLogAction.Failed, entityType: MigrationEntityType.Clients, page: 2, pageSize: 10 })
    expect(callOf(mock).url).toBe('/api/admin/migrations/3/logs?action=Failed&entityType=Clients&page=2&pageSize=10')
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Source unreachable.', 502))

    await expect(useMigrationApi().startJob(3)).rejects.toBeTruthy()
  })
})
