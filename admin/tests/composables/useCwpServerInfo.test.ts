/**
 * Tests for the CWP server-info composable: a fetch fills `info`, a refusal keeps the API's own
 * sentence in `error` and leaves `info` as it was, and `loading` is cleared either way.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useCwpServerInfo } from '@/composables/useCwpServerInfo'
import { json, problem, stubFetch, stubOffline, callOf } from '../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useCwpServerInfo', () => {
  it('fetch reads the CWP endpoint and fills info', async () => {
    const mock = stubFetch(json({ hostname: 'srv' }))
    const { info, loading, error, fetch } = useCwpServerInfo()

    await fetch()
    expect(callOf(mock).url).toBe('/api/admin/integrations/cwp/server-info')
    expect(info.value).toEqual({ hostname: 'srv' })
    expect(loading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('fetch keeps the API sentence and the previous info on refusal', async () => {
    const { info, error, fetch } = useCwpServerInfo()
    stubFetch(json({ hostname: 'srv' }))
    await fetch()

    stubFetch(problem('Server unreachable.', 502))
    await fetch()
    expect(error.value).toBe('Server unreachable.')
    expect(info.value).toEqual({ hostname: 'srv' })
  })

  it('fetch keeps the no-answer sentence offline', async () => {
    stubOffline()
    const { error, fetch } = useCwpServerInfo()

    await fetch()
    expect(error.value).toBe('Could not reach the server. Check your connection and try again.')
  })
})
