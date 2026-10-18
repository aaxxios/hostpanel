/**
 * Tests for the demo API composable: path and verb through the real `useApi` transport.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useDemoApi } from '@/composables/apis/useDemoApi'
import { DemoAccountRole } from '@/types/demo'
import { callOf, json, stubFetch } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useDemoApi', () => {
  it('fetchDemo reads /public/demo', async () => {
    const body = {
      enabled: true,
      nextResetAt: '2030-01-01T00:00:00Z',
      accounts: [{ role: DemoAccountRole.Admin, email: 'a@x.test', password: 'pw' }],
    }
    const mock = stubFetch(json(body))

    expect(await useDemoApi().fetchDemo()).toEqual(body)
    expect(callOf(mock)).toMatchObject({ url: '/api/public/demo', method: 'GET' })
  })
})
