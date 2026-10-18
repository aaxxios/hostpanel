/**
 * Tests for the gateway log API composable: the list endpoint through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useGatewayLogApi } from '@/composables/apis/useGatewayLogApi'
import { GatewayLogResult } from '@/types/gatewaylog'
import { json, stubFetch, callOf } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useGatewayLogApi', () => {
  it('list pages the endpoint and omits empty filters', async () => {
    const mock = stubFetch(json({ items: [], totalCount: 0, page: 1, pageSize: 20 }))

    await useGatewayLogApi().list({ from: '', to: '', gateway: '', debugData: '', result: '' }, 1, 20)

    expect(callOf(mock).url).toBe('/api/gateway-logs?page=1&pageSize=20')
  })

  it('list sends the filters, widening the day range to whole days', async () => {
    const mock = stubFetch(json({ items: [], totalCount: 0, page: 2, pageSize: 10 }))

    await useGatewayLogApi().list(
      { from: '2026-10-01', to: '2026-10-02', gateway: 'bank', debugData: 'ref', result: GatewayLogResult.Pending },
      2,
      10,
    )

    const query = new URLSearchParams(callOf(mock).url.split('?')[1])
    expect(query.get('from')).toBe('2026-10-01T00:00:00Z')
    expect(query.get('to')).toBe('2026-10-02T23:59:59.999Z')
    expect(query.get('gateway')).toBe('bank')
    expect(query.get('debugData')).toBe('ref')
    expect(query.get('result')).toBe('Pending')
    expect(query.get('page')).toBe('2')
  })
})
