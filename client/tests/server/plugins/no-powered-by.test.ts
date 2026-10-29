/**
 * Tests for `server/plugins/no-powered-by.ts`: the plugin registers a `beforeResponse` hook that
 * removes the `x-powered-by` header the renderer sets.
 *
 * @module tests/server/plugins/no-powered-by.test
 */

import { beforeAll, describe, expect, it, vi } from 'vitest'

/** The hook callbacks the plugin registered, by hook name. */
const hooks: Record<string, (event: unknown) => void> = {}

beforeAll(async () => {
  vi.stubGlobal('defineNitroPlugin', (plugin: (app: unknown) => void) => plugin)
  const plugin = (await import('~/server/plugins/no-powered-by')).default as unknown as (app: unknown) => void
  plugin({ hooks: { hook: (name: string, callback: (event: unknown) => void) => { hooks[name] = callback } } })
})

describe('no-powered-by plugin', () => {
  it('registers a beforeResponse hook', () => {
    expect(hooks['beforeResponse']).toBeTypeOf('function')
  })

  it('removes the x-powered-by header from the response', () => {
    const removeHeader = vi.fn()

    // Non-null: the first test asserts the hook was registered in beforeAll.
    hooks['beforeResponse']!({ node: { res: { removeHeader } } })

    expect(removeHeader).toHaveBeenCalledWith('x-powered-by')
  })
})
