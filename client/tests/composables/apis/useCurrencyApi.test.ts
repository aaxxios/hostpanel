/**
 * Tests for `composables/apis/useCurrencyApi.ts`.
 *
 * Each endpoint function is called for real through `apiFetch` against a stub server, and the
 * test asserts on the path, verb, query and body that arrived and on how the reply was typed.
 *
 * @module tests/composables/apis/useCurrencyApi.test
 */

import { afterEach, describe, expect, it } from 'vitest'
import { expectLastRequest, startTransport } from '../../helpers/stubTransport'
import type { ExpectedRequest } from '../../helpers/stubTransport'
import type { StubServer } from '../../helpers/stubServer'
import { useCurrencyApi } from '~/composables/apis/useCurrencyApi'

/** The stub started by the current test. */
let stub: StubServer | undefined

afterEach(async () => {
  await stub?.close()
  stub = undefined
})

/** The api surface under test. */
const api = useCurrencyApi()

/** Every endpoint function with the request it must send. */
const rows: ExpectedRequest[] = [
  { name: 'loadBillingCurrencies reads the operator currencies', call: () => api.loadBillingCurrencies(), method: 'GET', url: '/api/portal/public/billing-currencies' },
]

describe('useCurrencyApi', () => {
  it.each(rows)('$name', async (row) => {
    stub = await startTransport()

    await row.call()

    expectLastRequest(stub, row)
  })

  it('returns the configured currencies, base first', async () => {
    stub = await startTransport(() => ({ body: [{ code: 'AMD', isBase: true }] }))

    expect((await api.loadBillingCurrencies())[0]?.code).toBe('AMD')
  })
})
