/**
 * Tests for the reports API composable: verbs and paths, through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useReportsApi } from '@/composables/apis/useReportsApi'
import { json, problem, stubFetch } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useReportsApi', () => {
  it('fetchReport issues a GET on the given path', async () => {
    const mock = stubFetch(json({ a: 1 }))

    expect(await useReportsApi().fetchReport<{ a: number }>('/reports/x?y=1')).toEqual({ a: 1 })
    const init = mock.mock.calls[0]?.[1] as RequestInit | undefined
    expect(String(mock.mock.calls[0]?.[0])).toContain('/reports/x?y=1')
    expect(init?.method ?? 'GET').toBe('GET')
  })

  it('refreshReport issues a POST', async () => {
    const mock = stubFetch(json({ a: 2 }))

    expect(await useReportsApi().refreshReport<{ a: number }>('/reports/x')).toEqual({ a: 2 })
    expect((mock.mock.calls[0]?.[1] as RequestInit | undefined)?.method).toBe('POST')
  })

  it('fetchClientPicker reads /reports/client-picker', async () => {
    const mock = stubFetch(json([{ id: 1, name: 'Acme' }]))

    expect(await useReportsApi().fetchClientPicker()).toEqual([{ id: 1, name: 'Acme' }])
    expect(String(mock.mock.calls[0]?.[0])).toContain('/reports/client-picker')
  })

  it('exportUrl puts the transport prefix before the export path', () => {
    expect(useReportsApi().exportUrl('/reports/invoices/export?status=Paid')).toBe('/api/reports/invoices/export?status=Paid')
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('No access.', 403))

    await expect(useReportsApi().fetchReport('/reports/x')).rejects.toBeTruthy()
  })
})
