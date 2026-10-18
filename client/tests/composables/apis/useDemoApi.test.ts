/**
 * Tests for `composables/apis/useDemoApi.ts`.
 *
 * The endpoint function is called for real through `apiFetch` against a stub server, and the test
 * asserts on the path and verb that arrived and on how the reply was typed.
 *
 * @module tests/composables/apis/useDemoApi.test
 */

import { afterEach, describe, expect, it } from 'vitest'
import { expectLastRequest, startTransport } from '../../helpers/stubTransport'
import type { StubServer } from '../../helpers/stubServer'
import { useDemoApi } from '~/composables/apis/useDemoApi'

/** The stub started by the current test. */
let stub: StubServer | undefined

afterEach(async () => {
  await stub?.close()
  stub = undefined
})

/** The api surface under test. */
const api = useDemoApi()

describe('useDemoApi', () => {
  it('fetchDemo reads the demo status from the BFF route', async () => {
    stub = await startTransport()

    await api.fetchDemo()

    expectLastRequest(stub, { name: 'fetchDemo', call: () => api.fetchDemo(), method: 'GET', url: '/api/portal/demo' })
  })

  it('returns the status as the endpoint promises', async () => {
    stub = await startTransport(() => ({
      body: { enabled: true, nextResetAt: '2030-01-01T00:00:00Z', accounts: [{ role: 'Client', email: 'a@b.c', password: 'x' }] }
    }))

    const status = await api.fetchDemo()

    expect(status.enabled).toBe(true)
    expect(status.accounts[0]?.email).toBe('a@b.c')
  })
})
