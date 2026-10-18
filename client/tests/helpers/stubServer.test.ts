/**
 * Sample test proving the test infrastructure: happy-dom, the setup globals, and the stub server.
 *
 * @module tests/helpers/stubServer.test
 */

import { afterEach, describe, expect, it } from 'vitest'
import { startStubServer } from './stubServer'
import type { StubServer } from './stubServer'

/** The stub started by the current test. */
let stub: StubServer | undefined

afterEach(async () => {
  await stub?.close()
  stub = undefined
})

describe('test infrastructure', () => {
  it('runs in a DOM and exposes the auto-import stand-ins', () => {
    expect(typeof document.createElement).toBe('function')
    expect(typeof ref).toBe('function')
    expect(useI18n().t('some.key')).toBe('some.key')
  })

  it('records what reached the stub server and returns the scripted reply', async () => {
    stub = await startStubServer(() => ({ status: 201, body: { ok: true } }))

    const response = await fetch(`${stub.baseUrl}/ping`, { method: 'POST', headers: { 'x-language': 'ru' }, body: 'hi' })

    expect(response.status).toBe(201)
    expect(await response.json()).toEqual({ ok: true })
    expect(stub.requests).toHaveLength(1)
    expect(stub.requests[0]).toMatchObject({ method: 'POST', url: '/ping', body: 'hi' })
    expect(stub.requests[0]?.headers['x-language']).toBe('ru')
  })
})
