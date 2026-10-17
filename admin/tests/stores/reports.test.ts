/**
 * Tests for the reports store: data on success, null plus the API sentence on refusal,
 * flags reset, picker failure silent.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useReportsStore } from '@/stores/reports'

/** Builds a JSON response. */
const json = (body: unknown, status = 200): Response => new Response(JSON.stringify(body), { status })

/** A problem+json refusal carrying a finished sentence. */
const refusal = (): Response =>
  json({ type: 'REPORT_REFUSED', title: 'Refused', status: 403, detail: 'No access.' }, 403)

describe('reportsStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('fetchReport returns the data and clears loading', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(json({ a: 1 })))
    const store = useReportsStore()

    expect(await store.fetchReport<{ a: number }>('/reports/x')).toEqual({ a: 1 })
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('fetchReport returns null with the API sentence on refusal', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(refusal()))
    const store = useReportsStore()

    expect(await store.fetchReport('/reports/x')).toBeNull()
    expect(store.error).toBe('No access.')
    expect(store.loading).toBe(false)
  })

  it('fetchReport falls back when nothing answered', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')))
    const store = useReportsStore()

    await store.fetchReport('/reports/x')
    expect(store.error).toBe('Could not reach the server. Check your connection and try again.')
  })

  it('fetchReports returns every result in order, null when one fails', async () => {
    const store = useReportsStore()
    vi.stubGlobal('fetch', vi.fn()
      .mockResolvedValueOnce(json([1]))
      .mockResolvedValueOnce(json([2])))
    expect(await store.fetchReports<[number[], number[]]>(['/a', '/b'])).toEqual([[1], [2]])

    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(refusal()))
    expect(await store.fetchReports<[number[], number[]]>(['/a', '/b'])).toBeNull()
    expect(store.error).toBe('No access.')
  })

  it('refreshReport posts and tracks updating', async () => {
    const fetchMock = vi.fn().mockResolvedValue(json({ ok: true }))
    vi.stubGlobal('fetch', fetchMock)
    const store = useReportsStore()

    expect(await store.refreshReport('/reports/x/update')).toEqual({ ok: true })
    expect(store.updating).toBe(false)
    expect(fetchMock.mock.calls[0]?.[1]).toMatchObject({ method: 'POST' })
  })

  it('refreshReport reports the shared no-answer sentence offline', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')))
    const store = useReportsStore()

    expect(await store.refreshReport('/reports/x/update')).toBeNull()
    expect(store.error).toBe('Could not reach the server. Check your connection and try again.')
  })

  it('fetchClientPicker returns [] on failure without raising an error', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')))
    const store = useReportsStore()

    expect(await store.fetchClientPicker()).toEqual([])
    expect(store.error).toBeNull()
  })

  it('exportUrl hands back the address of the export', () => {
    expect(useReportsStore().exportUrl('/reports/x/export')).toBe('/api/reports/x/export')
  })
})
