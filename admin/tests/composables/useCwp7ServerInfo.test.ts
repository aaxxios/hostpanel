/**
 * Tests for the CWP7 server-info composable: a fetch fills `info` and a refusal keeps the API's
 * own sentence in `error`.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useCwp7ServerInfo } from '@/composables/useCwp7ServerInfo'
import { json, problem, stubFetch, callOf } from '../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useCwp7ServerInfo', () => {
  it('fetch reads the CWP7 endpoint and fills info', async () => {
    const mock = stubFetch(json({ hostname: 'srv7' }))
    const { info, error, fetch } = useCwp7ServerInfo()

    await fetch()
    expect(callOf(mock).url).toBe('/api/admin/integrations/cwp7/server-info')
    expect(info.value).toEqual({ hostname: 'srv7' })
    expect(error.value).toBeNull()
  })

  it('fetch keeps the API sentence on refusal', async () => {
    stubFetch(problem('Server unreachable.', 502))
    const { info, error, fetch } = useCwp7ServerInfo()

    await fetch()
    expect(error.value).toBe('Server unreachable.')
    expect(info.value).toBeNull()
  })
})
