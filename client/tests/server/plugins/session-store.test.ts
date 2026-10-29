/**
 * Tests for `server/plugins/session-store.ts`: the plugin builds the store at startup, so a
 * Redis deployment without a session encryption key refuses to start.
 *
 * @module tests/server/plugins/session-store.test
 */

import { randomBytes } from 'node:crypto'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

// A stand-in client: the test checks the startup decision, not a connection to a real Redis.
vi.mock('ioredis', () => ({
  default: class FakeRedis {
    /** The connection state a real client reports; the plugin never reads it. */
    status = 'ready'
  },
}))

/** The shape of the plugin module: its default export runs the startup work. */
interface PluginModule {
  /** Runs the plugin. */
  default: () => void
}

beforeEach(() => {
  vi.resetModules()
  vi.stubGlobal('defineNitroPlugin', (plugin: () => void) => plugin)
})

afterEach(() => {
  vi.unstubAllEnvs()
})

/**
 * Runs the plugin against a runtime config.
 *
 * @param config - The runtime config the plugin sees.
 */
const runPlugin = async (config: Record<string, unknown>): Promise<void> => {
  vi.stubGlobal('useRuntimeConfig', () => config)
  const module = await import('~/server/plugins/session-store') as unknown as PluginModule
  module.default()
}

describe('session-store plugin', () => {
  it('starts with the in-process store when no Redis is configured', async () => {
    await expect(runPlugin({ sessionRedis: '' })).resolves.toBeUndefined()
  })

  it('refuses to start with Redis and no encryption key outside development', async () => {
    vi.stubEnv('NODE_ENV', 'production')

    await expect(runPlugin({ sessionRedis: 'localhost:6379', sessionEncryptionKey: '' })).rejects.toThrow('required')
  })

  it('starts with Redis and a valid key', async () => {
    vi.stubEnv('NODE_ENV', 'production')

    await expect(runPlugin({
      sessionRedis: 'localhost:6379',
      sessionEncryptionKey: randomBytes(32).toString('base64'),
    })).resolves.toBeUndefined()
  })
})
