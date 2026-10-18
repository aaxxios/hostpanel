/**
 * Tests for the network issues API composable: paths, verbs, query strings and bodies through
 * the real `useApi` transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useNetworkIssuesApi } from '@/composables/apis/useNetworkIssuesApi'
import { json, stubFetch, callOf } from '../../helpers/fetch'
import { NetworkIssuePriority, NetworkIssueStatus, NetworkIssueTab, NetworkIssueType } from '@/types/networkissue'

afterEach(() => {
  vi.unstubAllGlobals()
})

/** A valid network issue body. */
const payload = {
  title: 'Packet loss',
  type: NetworkIssueType.Server,
  server: 'srv-1',
  priority: NetworkIssuePriority.High,
  status: NetworkIssueStatus.Reported,
  startDate: '2026-10-14T08:00:00Z',
  endDate: null,
  description: 'Investigating',
}

describe('useNetworkIssuesApi', () => {
  it('listNetworkIssues sends paging and status, dropping an empty status', async () => {
    const mock = stubFetch(json({ items: [], totalCount: 0 }))
    const api = useNetworkIssuesApi()

    await api.listNetworkIssues({ page: 1, pageSize: 20, status: NetworkIssueTab.Open })
    await api.listNetworkIssues({ page: 1, pageSize: 20, status: null })
    expect(callOf(mock, 0).url).toBe('/api/network-issues?page=1&pageSize=20&status=Open')
    expect(callOf(mock, 1).url).toBe('/api/network-issues?page=1&pageSize=20')
  })

  it('getNetworkIssue reads one issue', async () => {
    const mock = stubFetch(json({ id: 2 }))

    expect(await useNetworkIssuesApi().getNetworkIssue(2)).toEqual({ id: 2 })
    expect(callOf(mock).url).toBe('/api/network-issues/2')
  })

  it('createNetworkIssue POSTs the payload and returns the id', async () => {
    const mock = stubFetch(json(8))

    expect(await useNetworkIssuesApi().createNetworkIssue(payload)).toBe(8)
    expect(callOf(mock)).toMatchObject({ url: '/api/network-issues', method: 'POST', body: payload })
  })

  it('updateNetworkIssue PUTs and deleteNetworkIssue DELETEs', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useNetworkIssuesApi()

    await api.updateNetworkIssue(2, payload)
    await api.deleteNetworkIssue(2)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/network-issues/2', method: 'PUT', body: payload })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/network-issues/2', method: 'DELETE' })
  })
})
